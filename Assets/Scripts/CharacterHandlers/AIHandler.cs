using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum GlobalState { UNAGGRO, AGGRO, DEAD }; //determines ai state tree area

[RequireComponent(typeof(Collider))] //cause hit registry requires colliders
public class AIHandler : CharacterHandler {    

    [Header("AI Core Components/SOs")]
    public PlayerHandler targetPlayer; 

    //stealth stuff
    [Header("Stealth stuff")]
    public List<PatrolWaypoint> patrolWaypoints; //where ai walks
    public float idleTimeAtWaypoint; //how long ai stays at each patrol waypoint
    public float spotTimerThreshold; //time it takes to go into aggro
    public float AIGlobalStateCheckRange = 30f; //range ai can sense other AI and their states

    [Header("Combat Stuff")]
    public float attackStartDistance;
    public float shoveDistance;

    //private stuff
    protected NavMeshAgent agent;
    protected AIStealthState stealthState; 
    public Detection Detection {get; private set; }
    public GlobalState GlobalState {get; set; } = GlobalState.UNAGGRO; //spawn/start as aggro (todo unless otherwise stated)
    public Vector3 NextWaypointLocation {get; private set;} 

    #region callbacks
    protected override void Start() {
        base.Start(); //all character stuff
        Detection = this.GetComponent<Detection>();
        agent = this.GetComponent<NavMeshAgent>();
        //if (patrolWaypoints.Any()) NextWaypointLocation = patrolWaypoints[0].transform.position; //set first patrol waypoint
        //SetStateDriver(new PatrolState(this, animator, agent));
        genericState = new DefaultCombatState(this, animator); //todo temp probably
    }  
    #endregion

    #region core
    protected override void TakeDamage(float damage, bool isStaggerable) {
        base.TakeDamage(damage, isStaggerable);

        //Debug.Log("taken damage");
        if(Health <= 0) { //UPON AI DEATH todo should this be in super class
            SetStateDriver(new DeathState(this, animator)); 
        }

    }
    #endregion

    #region AIFSM
    //everytime the state is changed, do an exit routine (if applicable), switch the state, then trigger start routine (if applicable)
    public void SetStateDriver(AIStealthState state) { 
        StartCoroutine(SetState(state));
    }

    private IEnumerator SetState(AIStealthState state) {
        if(stealthState != null) yield return StartCoroutine(stealthState.OnStateExit());
        stealthState = state;
        yield return StartCoroutine(stealthState.OnStateEnter());
    }

    //used when stealth is broken, aka "deleting"
    public void ThrowStateToGCDriver() {
        StartCoroutine(ThrowStateToGC());
    }

    private IEnumerator ThrowStateToGC() {
        yield return StartCoroutine(stealthState.OnStateExit());
        stealthState = null;
    }
    #endregion

    #region stealthstuff
    //verify if stealth is valid
    public BTStatus VerifyStealth() {
        if(GlobalState == GlobalState.AGGRO) return BTStatus.FAILURE; 

        //cast a sphere, if any AI in that sphere is Aggro, turn into aggro as well
        Collider[] aiInRange = Physics.OverlapSphere(transform.position, AIGlobalStateCheckRange, Detection.obstacleMask);
            foreach(Collider col in aiInRange) {
            AIHandler proximateAI = col.GetComponent<AIHandler>();
            if(proximateAI.GlobalState == GlobalState.AGGRO){
                GlobalState = GlobalState.AGGRO;
                return BTStatus.FAILURE;
            }
        }

        return BTStatus.SUCCESS;
    }

    //check if ai has line of sight on player
    public bool LOSOnPlayer() {
        return Detection.VisibleTargets.Count != 0;
    }
    
    //move to next patrol point
    private int currPatrolIndex;
    public void SetNextPatrolDestination() { 
        currPatrolIndex = (currPatrolIndex + 1) % patrolWaypoints.Count;
        NextWaypointLocation = patrolWaypoints[currPatrolIndex].transform.position;
    }
    #endregion
    
    #region combatstuff
    //deals with AI BT if combat is still a viable option
    public bool VerifyCombatIncapable() { 
        //if not combat capable (either dead or pu55y), fail
        //otherwise, run OVERRIDE method from child class
        return GlobalState == GlobalState.DEAD;

    }

    public bool DefenceConditional(){
        return Detection.VisibleTargets.Any() //if i can see player
        && Detection.VisibleTargets[0].GetComponent<CharacterHandler>().genericState is AttackState //player is attacking
        && Detection.VisibleTargets[0].GetComponent<CharacterHandler>().FindTarget(((Detection.VisibleTargets[0].GetComponent<CharacterHandler>().genericState) as AttackState).chosenMove) == this //player is attacking me in particular
        && !(genericState is AttackState); //im not already mid attack      
    }

    public bool CloseDistanceConditional() { //returns FALSE if player is too far
        return (targetPlayer.transform.position - transform.position).sqrMagnitude <= attackStartDistance * attackStartDistance;
    }

    public bool InstantShoveConditional() { //returns FALSE if player is too close
        return (targetPlayer.transform.position - transform.position).sqrMagnitude >= shoveDistance * shoveDistance;
    }

    public bool StaggerCheck() {
        return !(genericState is StaggerState);
    }

    IEnumerator mainRoutine;
    // public BTStatus NormalDefense() {
    //     if(mainRoutine )
    //     return BTStatus.RUNNING
    // }

    private IEnumerator Block() {
        SetStateDriver(new BlockState(this, animator));
        yield return new WaitForSeconds(3f);
        SetStateDriver(new DefaultCombatState(this, animator));
    }

    /*
    start a combat cycle -> dealting with offensive and defensive behaviors
    each combat cycle ends when a certain criteria is met (ex. hitting the player and waiting a bit, blockign the player, etc)
    ienumeration hierarchy:
        currEngagementCoroutine // the cycle
            currEngagementAction // the general action the ai is taking, such as the attack or defence process
                currLocal...Action //the current action being took to complete an engagement action
        currLookCoroutine // controls enemy look direction, can go anywhere honestly
    */

/**
    IEnumerator currEngagementCycle; //the actual engagement cycle
    IEnumerator currEngagementAction; //the chosen engagement action, to complete engagement cycle
    IEnumerator currSubEngagementAction; //current action to complete engagement actoin
    IEnumerator currLookCoroutine; //deals with enemy look direction

    //upon verifying i am ready for combat
    public BTStatus EngageDriver() {
        if(currEngagementCycle == null) {
            currEngagementCycle = Engage();
            StartCoroutine(currEngagementCycle);
        }

        if(currLookCoroutine == null) {
            currLookCoroutine = FacePlayer();
            StartCoroutine(currLookCoroutine);
        }

        return BTStatus.RUNNING;
    }

    protected IEnumerator Engage() {
        //Debug.Log("engaging...");

        //if i am staggering, do stagger related thing
        if(genericState is StaggerState) {
            //when staggering, stop chasing for an attack and do something else (todo make it its own engagementAction)
            currEngagementAction = StaggerAction();
            yield return StartCoroutine(currEngagementAction);
        }
        //else, attack as normal
        else {
            //todo if the player is already close to the ai, do a close-range attack

            MeleeMove chosenAttack = weapon.Attacks[Random.Range(0, weapon.Attacks.Count)];
            currEngagementAction = OffensiveAction(chosenAttack); //first thought is to begin an offensive action
            StartCoroutine(currEngagementAction); //note - this function will know when to stop the cycle by itself
            

            //if i am attacked anytime in the process, happened to have died, or am currently staggering consider a defensive option. Otherwise, 
            yield return new WaitUntil(() => EngageInterrupt(chosenAttack) || GlobalState == GlobalState.DEAD || genericState is StaggerState);

            //situation 1, ive died
            if(GlobalState == GlobalState.DEAD) {
                agent.SetDestination(this.transform.position); //stop moving

                //stop appropraite coroutines
                if(currSubEngagementAction != null) StopCoroutine(currSubEngagementAction); 
                StopCoroutine(currEngagementAction); 
            }        
            
            //situatino 2, if i am hit mid offensive action
            else if(genericState is StaggerState) {
                //when staggering, stop chasing for an attack and do something else (todo make it its own engagementAction)

                //stop appropraite coroutines
                if(currSubEngagementAction != null) StopCoroutine(currSubEngagementAction);
                StopCoroutine(currEngagementAction); 

                currEngagementAction = StaggerAction();
                yield return StartCoroutine(currEngagementAction);
            }

            //situation 3, i should block
            //determine randomly via block quota if ai is to block
            else if(Random.Range(0, 1) < characterdata.blockQuota) {
                //stop local move, deal with set position as well
                agent.SetDestination(this.transform.position);

                if(currSubEngagementAction != null) StopCoroutine(currSubEngagementAction);
                StopCoroutine(currEngagementAction);

                //start and wait out a defensive action
                currEngagementAction = DefensiveAction();
                yield return StartCoroutine(currEngagementAction);
            } 
        }
    }

    //checks if theres reason to engage a defensive interrupt
    private bool EngageInterrupt(MeleeMove meleeMove) {
        return Detection.VisibleTargets.Any() //if i can see a cunt
        && Detection.VisibleTargets[0].GetComponent<CharacterHandler>().genericState is AttackState //cunts attacking
        && Detection.VisibleTargets[0].GetComponent<CharacterHandler>().FindTarget(meleeMove) == this //cunts attacking me in particular
        && !(genericState is AttackState); //im not already mid swing        
    }

    //offensive engagement
    protected virtual IEnumerator OffensiveAction(MeleeMove chosenAttack) {

        //close the distance to the player
        //Debug.Log("AI is closing distance to player");
        currSubEngagementAction = ChasePlayer(chosenAttack.range);
        yield return StartCoroutine(currSubEngagementAction);

        //Debug.Log("AI attempting an attack...");
        SetStateDriver(new AttackState(this, animator, chosenAttack));

        //once attack is finished
        yield return new WaitUntil(() => genericState is DefaultCombatState);

        //Debug.Log("attack finished, spacing from target");
        currSubEngagementAction = SpaceFromPlayer();
        yield return StartCoroutine(currSubEngagementAction);

        StopCoroutine(currEngagementCycle);
        currEngagementCycle = null;
    }

    protected virtual IEnumerator DefensiveAction(){
        Debug.Log("defending...");

        SetStateDriver(new BlockState(this, animator));
        yield return new WaitForSeconds(3f); //aka block time
        SetStateDriver(new DefaultCombatState(this, animator));

        currEngagementCycle = null; //once done with defensive action, restart cycle
    }

    protected virtual IEnumerator StaggerAction(){
        Debug.Log("stagger action");

        currSubEngagementAction = SpaceFromPlayer();
        yield return StartCoroutine(currSubEngagementAction);
        currEngagementCycle = null; //once done, go rethink

    }

    //sub actions
    private IEnumerator ChasePlayer(float attackRange) {
        agent.SetDestination(targetPlayer.transform.position);
        while(attackRange < agent.remainingDistance || agent.pathPending){
            agent.SetDestination(targetPlayer.transform.position);
            yield return new WaitForSeconds(.2f);
        }

        //sotp guy from movin aboot
        agent.SetDestination(this.transform.position);
        currSubEngagementAction = null;   
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

    //look stuff
    private IEnumerator FacePlayer() {
        //TODO: only when in combat
        while(GlobalState != GlobalState.DEAD) {
            Vector3 dirVec = targetPlayer.transform.position - transform.position;
            dirVec.y = 0;
            Quaternion facePlayerRotation = Quaternion.LookRotation(dirVec);
            transform.rotation = Quaternion.Lerp(transform.rotation, facePlayerRotation, 1);
            yield return null;
        }
    }

**/
    #endregion

    #region self preservation
    public BTStatus SelfPreserve() {
        return BTStatus.RUNNING;
        //if character is dead, fail
    }

    #endregion
    
}   