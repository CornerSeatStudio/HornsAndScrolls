using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum WeaponType { MELEE, RANGED };
public enum AIGlobalState { UNAGGRO, AGGRO, DEAD };
public enum CombatSlot {GUARANTEE, EVICTION, IN, OUT};

[System.Serializable] public class AISpecificEvent : UnityEvent<AIHandler>{}

public class AIHandler : CharacterHandler {    

    //core
    [Header("AICore")]
    public CharacterHandler target; 
    public WeaponData weapon;
    public WeaponType weaponType;
    public AISpecificEvent onTakeDamage;
    public AIGlobalState GlobalState {get; set; } = AIGlobalState.UNAGGRO;
    public float Priority { get; set; }
    public Dictionary<string, int> AnimationHashes { get; private set; }
    private List<AIHandler> proximateAI;


    //nav stuff
    [Header("Nav Core")]
    protected NavMeshAgent agent;
    private bool chaseInitializer = true;

    //stealth stuff
    [Header("Stealth stuff")]
    public List<PatrolWaypoint> patrolWaypoints;
    public float idleTimeAtWaypoint;
    public float spotTimerThreshold;
    private int currPatrolIndex;
    public Vector3 NextWaypointLocation {get; private set;} 

    //combat stuff
    [Header("Combat stuff")]
    public int combatPocket;
    public CombatSlot CombatSlot {get; set;} = CombatSlot.OUT;

    protected override void Start() {
        base.Start();
        agent = this.GetComponent<NavMeshAgent>();

        //initialize animation stuff as hash (more efficient)
        //uses a dict for organization - O(1) access (probably a hash table)
        AnimationHashes.Add("IsPatrol", Animator.StringToHash("IsPatrol"));
        AnimationHashes.Add("IsAggroWalk", Animator.StringToHash("IsAggroWalk"));
        AnimationHashes.Add("IsSearching", Animator.StringToHash("IsSearching"));
        AnimationHashes.Add("IsStaring", Animator.StringToHash("IsStaring"));


        NextWaypointLocation = patrolWaypoints[0].transform.position; //set first patrol waypoint
        SetStateDriver(new PatrolState(this, animator, agent));

    }

    public override void TakeDamage(float damage) {
        base.TakeDamage(damage);
        //release an event indicating x has taken damage
        onTakeDamage.Invoke(this);
    }

    public bool LOSOnPlayer() {
        return HitDetection.VisibleTargets.Count != 0;
    }
    
    //non combat tree

    public BTStatus VerifyStealth() { //verify if stealth is valid
        //todo: also check for transitions DIRECTLY to aggro
        return GlobalState == AIGlobalState.AGGRO? BTStatus.FAILURE : BTStatus.RUNNING;
    }

    public void SetNextPatrolDestination() { 
        currPatrolIndex = (currPatrolIndex + 1) % patrolWaypoints.Count;
        NextWaypointLocation = patrolWaypoints[currPatrolIndex].transform.position;
    }


    //combat tree stuff
    public BTStatus ChasePlayer() { 
        //unfreeze cause im lazy
        if (HitDetection.InMeleeRoutine) { agent.isStopped = true;} else { agent.isStopped = false;}
        //chase the player until within hit range
        if(chaseInitializer){
            StartCoroutine("ChaseCoroutine");
            chaseInitializer = false;
        }
        
        //if within range
        if((transform.position - target.transform.position).sqrMagnitude < HitDetection.interactionDistance * HitDetection.interactionDistance){
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