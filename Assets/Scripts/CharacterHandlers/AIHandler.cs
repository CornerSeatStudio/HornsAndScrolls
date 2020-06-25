using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum WeaponType { MELEE, RANGED };
public enum AIState { IDLE, PATROL, COMBAT, DEAD }; 
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
    private bool finishedIdlingAtPatrolPoint = true;
    private bool travellingToPatrolWaypoint;
    private bool readyToStartPatrolWaypointCountdown = true;
    private int currPatrolIndex;
    private Vector3 nextWaypointLocation;
    private bool patrolAlreadyHalted = false;
    private bool patrolAlreadyResumed = true;
    private bool isDecrement;
    private bool inTimerCoroutine = false;

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
    public bool ValidifyCombatState() {return true; }//if this succeeds, go to combat routine, otherwise, continue stealth routine 
    public BTStatus CheckAndMaintainLOS() {
        //if line of sight isn't achieved/is broken, return failure (removes need of PlayerInLOS)
        if(hitDetection.VisibleTargets.Count == 0) { return BTStatus.FAILURE; }
        
        if(!patrolAlreadyHalted) { // check if ai needs to be halted - if so, halt
            HaltPatrol();
            patrolAlreadyHalted = true;
            patrolAlreadyResumed = false;
        }

        //only start decrementing when player is in LOS, DONT trigger if already started
        isDecrement = true;
        if (!inTimerCoroutine) StartCoroutine("AdjustSpotTimer"); 

        //stand and stare at player

        return BTStatus.RUNNING; 
    } 
    public BTStatus ResumePatrolAsNormal() {
        if (patrolAlreadyResumed) { return BTStatus.FAILURE; } //if no reason to resume patrol, fail


        //if not, make coroutine increment instead
        isDecrement = false;

        //once it finishes (via flag), resume patrol
        if(!inTimerCoroutine) {
            ResumePatrol();
            patrolAlreadyResumed = true;
            patrolAlreadyHalted = false;
            return BTStatus.SUCCESS;
        }

        return BTStatus.RUNNING;
    }
    public BTStatus ExecutePatrol() { //deal with stopping at each waypoint
        //if there are no points, just do idle
        if (!patrolWaypoints.Any() || patrolWaypoints == null) { return Idle(); }

        //if idle time over, walk to next patrol waypoint, reset timer
        if (finishedIdlingAtPatrolPoint) { 
            //todo turn and face direction of waypoint
            ResumePatrol();
        }
        //once arrived, halt, 
        if (travellingToPatrolWaypoint && (transform.position - GetCurrPatrolDestination()).sqrMagnitude < 200f) {
            HaltPatrol();
        }
        
        //idle at patrol waypoint for idle timer duration
        //if player is no longer travelling to waypoint and coroutine hasnt already started
        if (!travellingToPatrolWaypoint && readyToStartPatrolWaypointCountdown) {
            readyToStartPatrolWaypointCountdown = false;
            nextWaypointLocation = SetNextPatrolDestination();
            StartCoroutine("IdleAtPatrolWaypoint", idleTimeAtWaypoint);
            //todo whatever cunt does idle at patrol point should happen here
        }
        
        return BTStatus.RUNNING;

    }
    private IEnumerator AdjustSpotTimer() { //note -> the += or -= must be by a number that can fold into an integer
        inTimerCoroutine = true;
        while (spotTimer > 0 && spotTimer < spotTimerThreshold && (isDecrement || spotTimer % spotTimerDivisions != 0)) {
            spotTimer = isDecrement ? spotTimer -.2f : spotTimer +.2f;
            yield return new WaitForSeconds(.2f);
        }
        inTimerCoroutine = false;
    }
    private void HaltPatrol() {
        travellingToPatrolWaypoint = false;
        StopCoroutine("PatrolCoroutine");
    }
    private void ResumePatrol() {
        finishedIdlingAtPatrolPoint = false;
        travellingToPatrolWaypoint = true;
        StartCoroutine("PatrolCoroutine");
    }
    private Vector3 GetCurrPatrolDestination() {
        return patrolWaypoints[currPatrolIndex].transform.position;
    }
    private Vector3 SetNextPatrolDestination() { 
        currPatrolIndex = (currPatrolIndex + 1) % patrolWaypoints.Count;
        return patrolWaypoints[currPatrolIndex].transform.position;
    }
    private IEnumerator PatrolCoroutine() { //deals with the walking to a waypoint
        while (true) {
            agent.SetDestination(nextWaypointLocation);
            yield return null;
        }
    }
    private IEnumerator IdleAtPatrolWaypoint(float idleTimeAtWaypoint) { //idle behavior at given waypoint goes here
        //Debug.Log("idling at patrol point");
        yield return new WaitForSeconds(idleTimeAtWaypoint);
        readyToStartPatrolWaypointCountdown = true;
        finishedIdlingAtPatrolPoint = true;
    }
    public BTStatus Idle() {
        return BTStatus.RUNNING;
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