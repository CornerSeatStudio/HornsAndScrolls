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
    private int currPatrolIndex;
    private Vector3 nextWaypointLocation;
    private IEnumerator currStealthCoroutine;
    private float currInvestigationTimer = 0f;
    private bool isDecrement = false;
    private Vector3 lastSeenPlayerLocation;

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

    //note: for all stealth coroutines, I use a single private ienumerator
    //i must mannually make it so that there is only ONE STEALTH COROUTINE at once

    //non combat tree
    public BTStatus StealthRoutine() {
        Debug.Log(AIState);
        //Debug.Log(currStealthCoroutine);
        switch (AIState) {
            case(AIState.COMBAT):
                return BTStatus.FAILURE; 
            
            case(AIState.INVESTIGATING):
                return Investigating();

            case(AIState.DEAD) :
                return BTStatus.SUCCESS; //todo

            default: 
                return PatrolAndIdle();
        }
   
    }

    private BTStatus PatrolAndIdle() {
        //if interrupted, go to investigate
        if(hitDetection.VisibleTargets.Count != 0) { 
            SwitchToInvestigation();
        }

        //once reached destination, start idle coroutine, set enum
        else if (currStealthCoroutine == null && AIState == AIState.PATROL) {
            SwitchToIdle();
        }

        //once timer runs up, start patrol coroutine
        else if (currStealthCoroutine == null && AIState == AIState.IDLE) {
            nextWaypointLocation = SetNextPatrolDestination(); //set the next destination
            SwitchToPatrol();
        }
        
        //otherwise, do patrol/idle as normal
        return BTStatus.RUNNING;
    }
    private BTStatus Investigating() { //note: currInvestigationTimer == spotTimerThreshold == spotted
        //in in line of sight:
        if(hitDetection.VisibleTargets.Count != 0) { 
            //ensure incrementation
            isDecrement = false;
            //stand still and face player
            agent.isStopped = true;
            //if timer is full, change to combat
            if(currInvestigationTimer >= spotTimerThreshold) { 
                AIState = AIState.COMBAT;
                return BTStatus.FAILURE;
            }

            //the moment the timer exceeds x threshold, start recording player position
            if(spotTimerThreshold/2 < currInvestigationTimer) {
                lastSeenPlayerLocation =  target.transform.position;
            }

        }
        //if line of sight is broken
        else {
            //if timer hits 0, go back to patrol
            if(currInvestigationTimer < 0) { 
                agent.isStopped = false;
                SwitchToPatrol(); //doesnt change last patrol waypoint
                return BTStatus.SUCCESS;
            } else { //otherwise, do chosen behavior
                //reverse timer
                isDecrement = true; 
                //if under x threshold, do in place search
                if(spotTimerThreshold/2 > currInvestigationTimer){ //todo: this should branch into two decision made ONCE per loss of sight
                    agent.isStopped = true; //do a cheeky gander                
                } else { //if over x threshold,  
                    if (!chainedDiscoverStarted){
                        StartCoroutine("chainedDiscovery"); //parent coroutine, independed of currStealthCoroutine
                    }
                }
            }
        }
            
        return BTStatus.RUNNING;
    } 
    bool chainedDiscoverStarted = false;
    private IEnumerator chainedDiscovery() {
        chainedDiscoverStarted = true;
        Debug.Log("tiem for chained discovery");
        StopCoroutine(currStealthCoroutine); //pause timer
        agent.isStopped = false; //unfreeze movement

        yield return new WaitForSeconds(3f); //grace period before shit, put cheeky browse coroutine here

        currStealthCoroutine = MoveToLocation(lastSeenPlayerLocation); 
        yield return StartCoroutine(currStealthCoroutine); //go to saved position,

        agent.isStopped = true; //re freeze movement, cheeky browse goes here

        currStealthCoroutine = InvestigationTimer();
        yield return StartCoroutine(currStealthCoroutine); //restart timer,
        chainedDiscoverStarted = false;
    }


    private void SwitchToIdle() {
        AIState = AIState.IDLE;
        currStealthCoroutine = IdleCoroutine();
        StartCoroutine(currStealthCoroutine); 
    }
    private void SwitchToPatrol() {
        AIState = AIState.PATROL;
        currStealthCoroutine = MoveToLocation(nextWaypointLocation);
        StartCoroutine(currStealthCoroutine); 
    }
    private void SwitchToInvestigation() {
        StopCoroutine(currStealthCoroutine); //stop either the patrol or the idle timer
        currInvestigationTimer = 0.1f; //reset investigation meter
        AIState = AIState.INVESTIGATING;
        currStealthCoroutine = InvestigationTimer();
        StartCoroutine(currStealthCoroutine);
    }
    private IEnumerator InvestigationTimer() {
        while(currInvestigationTimer < spotTimerThreshold && currInvestigationTimer >= 0) {
            yield return new WaitForSeconds(.1f); //space between each investigation timer tick - should be the same as think cycle
            currInvestigationTimer = isDecrement? currInvestigationTimer -= .1f : currInvestigationTimer += .1f;
        }
        currStealthCoroutine = null;
    }
    private IEnumerator MoveToLocation(Vector3 location){ //shouldnt be a coroutine lol, no reason for the while statemtn
        while (!((transform.position - location).sqrMagnitude < 200f)) { //keep going until within reasonable distance
            agent.SetDestination(location);
            yield return new WaitForSeconds(.2f); //same as think cycle
        }
        // agent.SetDestination(location);
        currStealthCoroutine = null;
        // yield return null;
    }
    private IEnumerator IdleCoroutine() { //idle behavior at given waypoint goes here
        //Debug.Log("in idle coroutine");
        yield return new WaitForSeconds(idleTimeAtWaypoint);
        currStealthCoroutine = null;
    }
    private Vector3 SetNextPatrolDestination() { 
        currPatrolIndex = (currPatrolIndex + 1) % patrolWaypoints.Count;
        return patrolWaypoints[currPatrolIndex].transform.position;
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