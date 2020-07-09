using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum AIGlobalState { UNAGGRO, AGGRO, DEAD };
public enum CombatSlot {GUARANTEE, EVICTION, IN, OUT};

[RequireComponent(typeof(Collider))]
public class AIHandler : CharacterHandler {    

    //core - TODO REQUIRE BOX COLLIDER
    [Header("AI Core Components/SOs")]
    public CharacterHandler target;
    protected NavMeshAgent agent;
    public Detection Detection {get; private set; }

    //nav stuff
    [Header("AI Core Members")]
    protected AIState localState; //stealth state
    public AIGlobalState GlobalState {get; set; } = AIGlobalState.UNAGGRO; //for general labeling

    //stealth stuff
    [Header("Stealth stuff")]
    public List<PatrolWaypoint> patrolWaypoints;
    public float idleTimeAtWaypoint;
    public float spotTimerThreshold; //time it takes to go into aggro
    public float AIGlobalStateCheckRange = 30f;

    private int currPatrolIndex;
    public Vector3 NextWaypointLocation {get; private set;} 


    #region callbacks
    protected override void Start() {
        base.Start();
        Detection = this.GetComponent<Detection>();
        agent = this.GetComponent<NavMeshAgent>();
        //if (patrolWaypoints.Any()) NextWaypointLocation = patrolWaypoints[0].transform.position; //set first patrol waypoint
        //SetStateDriver(new PatrolState(this, animator, agent));
    }  
    
    //initialize animation stuff as hash (more efficient)
    //uses a dict for organization - O(1) access (probably a hash table)
    
    #endregion

    #region core
    protected override void TakeDamage(float damage) {
        base.TakeDamage(damage);

        if(Health <= 0) {
            SetStateDriver(new DeathState(this, animator, MeleeRaycastHandler)); //anything to do with death is dealt with here
        }

    }
    #endregion

    #region AIFSM
    //everytime the state is changed, do an exit routine (if applicable), switch the state, then trigger start routine (if applicable)
    public void SetStateDriver(AIState state) { 
        StartCoroutine(SetState(state));
    }

    private IEnumerator SetState(AIState state) {
        if(localState != null) yield return StartCoroutine(localState.OnStateExit());
        localState = state;
        yield return StartCoroutine(localState.OnStateEnter());
    }

    public void ThrowStateToGCDriver() {
        StartCoroutine(ThrowStateToGC());
    }

    private IEnumerator ThrowStateToGC() {
        yield return StartCoroutine(localState.OnStateExit());
        localState = null;
    }
    #endregion

    #region stealthstuff
    public BTStatus VerifyStealth() { //verify if stealth is valid
        if(GlobalState == AIGlobalState.AGGRO) return BTStatus.FAILURE;

        //cast a sphere, if any AI in that sphere is Aggro, turn into aggro as well
        Collider[] aiInRange = Physics.OverlapSphere(transform.position, AIGlobalStateCheckRange, Detection.obstacleMask);
            foreach(Collider col in aiInRange) {
            AIHandler proximateAI = col.GetComponent<AIHandler>();
            if(proximateAI.GlobalState == AIGlobalState.AGGRO){
                GlobalState = AIGlobalState.AGGRO;
                return BTStatus.FAILURE;
            }
        }

        return BTStatus.SUCCESS;
    }

    public bool LOSOnPlayer() {
        return Detection.VisibleTargets.Count != 0;
    }
    
    public void SetNextPatrolDestination() { 
        currPatrolIndex = (currPatrolIndex + 1) % patrolWaypoints.Count;
        NextWaypointLocation = patrolWaypoints[currPatrolIndex].transform.position;
    }
    #endregion
    
    #region combatstuff
    public bool VerifyCombatIncapable() { 
        //if not combat capable (either dead or pu55y), fail
        //otherwise, run OVERRIDE method from child class
        return GlobalState == AIGlobalState.DEAD;

    }

    //for bt
    IEnumerator currEngagementCoroutine;
    IEnumerator currLookCoroutine;
    public BTStatus EngageDriver() {
        //Debug.Log("engaging think loop");
        //begin an engagement instance - aka the prep and actual attack of an AI OR the defense response
        //if coroutine needs is running, return running
        if(currEngagementCoroutine == null) {
            currEngagementCoroutine = Engage();
            StartCoroutine(currEngagementCoroutine);
        }

        if(currLookCoroutine == null) {
            currLookCoroutine = FacePlayer();
            StartCoroutine(currLookCoroutine);
        }

        return BTStatus.RUNNING;
    }

    //core engagement loop
    IEnumerator currEngagementAction;
    protected IEnumerator Engage() {
        Debug.Log("engaging...");


        currEngagementAction = OffensiveAction();
        StartCoroutine(currEngagementAction);

        //if cunt is attacked anytime in the process
        yield return new WaitUntil(() => EngageInterrupt() || GlobalState == AIGlobalState.DEAD);

        if(GlobalState == AIGlobalState.DEAD) {
            agent.SetDestination(this.transform.position);
            if(currLocalOffensiveAction != null) StopCoroutine(currLocalOffensiveAction);
            StopCoroutine(currEngagementAction);
            yield break;
        }
        
        //determine randomly via block quota if ai is to block
        if(Random.Range(0, 1) < characterdata.blockQuota) {
            //stop local move, deal with set position as well
            agent.SetDestination(this.transform.position);
            if(currLocalOffensiveAction != null) StopCoroutine(currLocalOffensiveAction);
            //stop the curr engagement action
            StopCoroutine(currEngagementAction);

            //start and wait out a defensive action
            currEngagementAction = DefensiveAction();
            yield return StartCoroutine(currEngagementAction);

            currEngagementCoroutine = null; //once done, go rethink
        }
    }

    //checks if theres reason to engage a defensive interrupt
    protected bool EngageInterrupt() {
        return Detection.VisibleTargets.Any() //if i can see a cunt
        && Detection.VisibleTargets[0].GetComponent<CharacterHandler>().combatState is AttackState //cunts attacking
        && Detection.VisibleTargets[0].GetComponent<CharacterHandler>().MeleeRaycastHandler.chosenTarget == this //cunts attacking me in particular
        && !(combatState is AttackState); //im not already mid swing

        //IF THEY BE MSISING IS IPORTANT TOO
        
    }

    MeleeMove chosenAttack;
    IEnumerator currLocalOffensiveAction;
    protected virtual IEnumerator OffensiveAction() {
        //if the player is already close to the ai, do a close-range attack

        //otherwise, pick an offensive attack move at random
        chosenAttack = weapon.Attacks[Random.Range(0, weapon.Attacks.Count)];

        //close the distance to the player
        
        Debug.Log("AI is closing distance to player");

        currLocalOffensiveAction = ChasePlayer();
        yield return StartCoroutine(currLocalOffensiveAction);

        Debug.Log("AI attempting an attack...");
        SetStateDriver(new AttackState(this, animator, MeleeRaycastHandler, chosenAttack));

        //once attack is finished
        yield return new WaitUntil(() => combatState is DefaultState);

        Debug.Log("attack finished, spacing from target");
        currLocalOffensiveAction = SpaceFromPlayer();
        yield return StartCoroutine(currLocalOffensiveAction);

        StopCoroutine(currEngagementCoroutine);
        currEngagementCoroutine = null;
    }

    private IEnumerator ChasePlayer() {
        agent.SetDestination(target.transform.position);
        while(chosenAttack.range < agent.remainingDistance || agent.pathPending){
            agent.SetDestination(target.transform.position);
            yield return new WaitForSeconds(.2f);
        }

        //sotp guy from movin aboot
        agent.SetDestination(this.transform.position);
        currLocalOffensiveAction = null;   
    }

    private IEnumerator SpaceFromPlayer() {
        agent.updateRotation = false; //prevents navmeshagent from manipulating rotation
        NavMeshHit hit;
        Vector3 movePosition = new Vector3();
        bool locationFound = false;
        //pick a random destination within a given area behind the character (only try 30 times to be safe)
        for(int i = 0; i < 30; ++i){
            if(NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas)){
                locationFound = true;
                movePosition = hit.position;
                break;
            }
        }

        //if hit was found:
        if(locationFound) {
            Debug.Log("sample location found, wait 5f");
            agent.SetDestination(movePosition);
            yield return new WaitForSeconds(5f);
        } else {
            Debug.Log("no sample location found, wait 5f");
            yield return new WaitForSeconds(5f);

        }

        agent.SetDestination(transform.position);
        agent.updateRotation = true;
    }

    private IEnumerator FacePlayer() {
        //TODO: only when in combat
        while(GlobalState != AIGlobalState.DEAD) {
            Vector3 dirVec = target.transform.position - transform.position;
            dirVec.y = 0;
            Quaternion facePlayerRotation = Quaternion.LookRotation(dirVec);
            transform.rotation = Quaternion.Lerp(transform.rotation, facePlayerRotation, 1);
            yield return null;
        }
    }

    protected virtual IEnumerator DefensiveAction(){
        Debug.Log("defending...");

        //FACE PLAYER DUMBASS

        SetStateDriver(new BlockState(this, animator, MeleeRaycastHandler));
        yield return new WaitForSeconds(3f); //aka block time
        SetStateDriver(new DefaultState(this, animator, MeleeRaycastHandler));
    }

    #endregion

    #region self preservation
    public BTStatus SelfPreserve() {
        return BTStatus.RUNNING;
        //if character is dead, fail
    }

    #endregion
    
}   