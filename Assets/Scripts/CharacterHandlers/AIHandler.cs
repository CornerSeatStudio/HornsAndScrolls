using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum AIGlobalState { UNAGGRO, AGGRO, DEAD };
public enum CombatSlot {GUARANTEE, EVICTION, IN, OUT};

[System.Serializable] public class AISpecificEvent : UnityEvent<AIHandler>{}

public class AIHandler : CharacterHandler {    

    //core - TODO REQUIRE BOX COLLIDER
    [Header("AI Core Components/SOs")]
    public CharacterHandler target;
    protected NavMeshAgent agent;
    public Detection Detection {get; private set; }

    //nav stuff
    [Header("AI Core Members")]
    public AISpecificEvent onTakeDamage;
    protected AIState localState; //stealth state

    public AIGlobalState GlobalState {get; set; } = AIGlobalState.UNAGGRO; //for general labeling
    public Dictionary<string, int> AnimationHashes { get; private set; }
    private List<AIHandler> proximateAI;

    //stealth stuff
    [Header("Stealth stuff")]
    public List<PatrolWaypoint> patrolWaypoints;
    public float idleTimeAtWaypoint;
    public float spotTimerThreshold; //time it takes to go into aggro
    
    private int currPatrolIndex;
    public Vector3 NextWaypointLocation {get; private set;} 

    //combat stuff
    [Header("Combat stuff")]
    public int combatPocket;
    public CombatSlot CombatSlot {get; set;} = CombatSlot.OUT;
    public float Priority { get; set; }


    #region callbacks
    protected override void Start() {
        base.Start();
        Detection = this.GetComponent<Detection>();
        agent = this.GetComponent<NavMeshAgent>();
        setupAnimationHashes();
        //if (patrolWaypoints.Any()) NextWaypointLocation = patrolWaypoints[0].transform.position; //set first patrol waypoint
        //SetStateDriver(new PatrolState(this, animator, agent));
    }  
    
    //initialize animation stuff as hash (more efficient)
    //uses a dict for organization - O(1) access (probably a hash table)
    private void setupAnimationHashes() {
        AnimationHashes = new Dictionary<string, int>();
        AnimationHashes.Add("IsPatrol", Animator.StringToHash("IsPatrol"));
        AnimationHashes.Add("IsAggroWalk", Animator.StringToHash("IsAggroWalk"));
        AnimationHashes.Add("IsSearching", Animator.StringToHash("IsSearching"));
        AnimationHashes.Add("IsStaring", Animator.StringToHash("IsStaring"));
    }
    #endregion

    #region core
    protected override void TakeDamage(float damage) {
        base.TakeDamage(damage);
        //release an event indicating x has taken damage
        onTakeDamage.Invoke(this);
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
        //todo: also check for transitions DIRECTLY to aggro
        return GlobalState == AIGlobalState.AGGRO? BTStatus.FAILURE : BTStatus.RUNNING;

        //if fails, STOP DETECTION todo
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

    IEnumerator currEngagementCoroutine;
    public BTStatus EngageDriver() {
        //begin an engagement instance - aka the prep and actual attack of an AI OR the defense response
        //if coroutine needs is running, return running
        if(currEngagementCoroutine == null) {
            currEngagementCoroutine = Engage();
            StartCoroutine(currEngagementCoroutine);
        }

        return BTStatus.RUNNING;
    }

    IEnumerator currEngagementAction;
    protected virtual IEnumerator Engage() {

        currEngagementAction = OffensiveAction();
        StartCoroutine(currEngagementAction);

        //if cunt is attacked anytime in the process
        yield return new WaitUntil(() => Detection.VisibleTargets.Any() && Detection.VisibleTargets[0].gameObject.GetComponent<CharacterHandler>().combatState is AttackState);

        //stop the curr engagement action
        StopCoroutine(currEngagementAction);

        //start and wait out a defensive action
        currEngagementAction = DefensiveAction();
        yield return StartCoroutine(currEngagementAction);

        currEngagementCoroutine = null; //once done, go rethink
    }

    protected virtual IEnumerator OffensiveAction() {
        yield return null;
        /*
        //if the player is already close to the ai, use a defensive action

        //otherwise, pick an offensive attack move at random

        //close the distance to the player
            //OR if times out, fail and idle for a hot sec

        //do the attack

        idle for x seconds
        */
    }

    protected virtual IEnumerator DefensiveAction(){
        SetStateDriver(new BlockState(this, animator, meleeRaycastHandler));
        yield return new WaitForSeconds(3f);
        SetStateDriver(new DefaultState(this, animator, meleeRaycastHandler));
    }

    public BTStatus NeutralAction() { //default action if engage fails
        //face player
        return BTStatus.RUNNING;
    }
    #endregion

    #region self preservation
    public BTStatus SelfPreserve() {
        return BTStatus.RUNNING;
        //if character is dead, fail
    }

    #endregion
    public BTStatus KillBrain() {
        return BTStatus.RUNNING;
        //upon death, kill think cycle (and all other shit)
    }
}   