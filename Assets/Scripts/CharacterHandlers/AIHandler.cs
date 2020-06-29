using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum WeaponType { MELEE, RANGED };
public enum AIState { IDLE, PATROL, INVESTIGATING, COMBAT, DEAD }; 
public enum CombatSlot {GUARANTEE, EVICTION, IN, OUT};

[System.Serializable] public class AISpecificEvent : UnityEvent<AIHandler>{}

public class AIHandler : CharacterHandler {    

    //core
    [Header("AICore")]
    public CharacterHandler target; 
    public WeaponData weapon;
    public WeaponType weaponType;
    [SerializeField] public AIState AIState {get; set; } = AIState.IDLE;
    private List<AIHandler> proximateAI;
    public float Priority { get; set; }
    public AISpecificEvent onTakeDamage;

    //nav stuff
    [Header("Nav Core")]
    protected NavMeshAgent agent;
    private bool chaseInitializer = true;

    //stealth stuff
    [Header("Stealth stuff")]
    public List<PatrolWaypoint> patrolWaypoints;
    public float idleTimeAtWaypoint;
    public float spotTimerThreshold;
    public float spotTimerDivisions;
    private float spotTimer;
    private int currPatrolIndex;
    private Vector3 nextWaypointLocation;
    private IEnumerator currStealthCoroutine;

    //combat stuff
    [Header("Combat stuff")]
    public int combatPocket;
    public CombatSlot CombatSlot {get; set;} = CombatSlot.OUT;

    protected override void Start() {
        base.Start();
        agent = this.GetComponent<NavMeshAgent>();
    }

    //core 
    public override void TakeDamage(float damage) {
        base.TakeDamage(damage);
        //release an event indicating x has taken damage
        onTakeDamage.Invoke(this);
    }

    //general behaviors


    //non combat tree
    public BTStatus StealthRoutine() {
        
        // Debug.Log(AIState);
        // Debug.Log(currStealthCoroutine);
        // Debug.Log(nextWaypointLocation);
        switch (AIState) {
            case(AIState.IDLE):
                return Idle();

            case(AIState.PATROL): 
                return Patrol(); 
            
            case(AIState.INVESTIGATING):
                return Investigating();

            default:
                return BTStatus.FAILURE;
        }
   
    }

    private BTStatus Patrol() {
        //if interrupted, go to investigate
        if(hitDetection.VisibleTargets.Count != 0) { 
            StopCoroutine(currStealthCoroutine);
            AIState = AIState.INVESTIGATING;
        }

        //once reached destination, start idle coroutine, set enum
        if (currStealthCoroutine == null) {
            AIState = AIState.IDLE;
            currStealthCoroutine = IdleCoroutine();
            StartCoroutine(currStealthCoroutine); 
        }
        
        //otherwise, do patrol as normal
        return BTStatus.RUNNING;
    }

    private BTStatus Idle() {
        //if interrupted, go to investigate
        if(hitDetection.VisibleTargets.Count != 0) { 
            StopCoroutine(currStealthCoroutine);
            AIState = AIState.INVESTIGATING;
        }
        //once timer runs up, start patrol coroutine
        if (currStealthCoroutine == null) { 
            AIState = AIState.PATROL;
            currStealthCoroutine = PatrolCoroutine();
            StartCoroutine(currStealthCoroutine); 
        }

        //otherwise, do the idle thing
        return BTStatus.RUNNING;
    }

    private BTStatus Investigating() {
        //start timer:


        return BTStatus.RUNNING;
    }

    // private IEnumerator AdjustSpotTimer() { //note -> the += or -= must be by a number that can fold into an integer
    //     inTimerCoroutine = true;
    //     while (spotTimer > 0 && spotTimer < spotTimerThreshold && (isDecrement || spotTimer % spotTimerDivisions != 0)) {
    //         spotTimer = isDecrement ? spotTimer -.2f : spotTimer +.2f;
    //         yield return new WaitForSeconds(.2f);
    //     }
    //     inTimerCoroutine = false;
    // }

    private Vector3 GetCurrPatrolDestination() {
        return patrolWaypoints[currPatrolIndex].transform.position;
    }
    private Vector3 SetNextPatrolDestination() { 
        currPatrolIndex = (currPatrolIndex + 1) % patrolWaypoints.Count;
        return patrolWaypoints[currPatrolIndex].transform.position;
    }
    
    private IEnumerator PatrolCoroutine() { //deals with the walking to a waypoint
        //Debug.Log("in patrol coroutine");
        nextWaypointLocation = SetNextPatrolDestination(); //set the next destination
        while (!((transform.position - GetCurrPatrolDestination()).sqrMagnitude < 200f)) { //keep going until within reasonable distance
            agent.SetDestination(nextWaypointLocation);
            yield return null;
        }

        currStealthCoroutine = null;
    }

    private IEnumerator IdleCoroutine() { //idle behavior at given waypoint goes here
        //Debug.Log("in idle coroutine");
        yield return new WaitForSeconds(idleTimeAtWaypoint);
        currStealthCoroutine = null;
    }


    //combat tree stuff
    public BTStatus ChasePlayer() { 
        //unfreeze cause im lazy
        if (hitDetection.InMeleeRoutine) { agent.isStopped = true;} else { agent.isStopped = false;}
        //chase the player until within hit range
        if(chaseInitializer){
            StartCoroutine("ChaseCoroutine");
            chaseInitializer = false;
        }
        
        //if within range
        if((transform.position - target.transform.position).sqrMagnitude < hitDetection.interactionDistance * hitDetection.interactionDistance){
            StopCoroutine("ChaseCoroutine");
            chaseInitializer = true;
            return BTStatus.SUCCESS;
        }

        return BTStatus.RUNNING;
     }
 
    private IEnumerator ChaseCoroutine(){
        while (true){
            agent.SetDestination(target.transform.position);
            yield return null;
        }
    }

    public BTStatus SpaceFromTarget() {return BTStatus.RUNNING;}

}