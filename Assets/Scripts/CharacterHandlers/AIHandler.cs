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
    [Header("AICore")]
    public CharacterHandler target;
    public WeaponData weapon;
    public AISpecificEvent onTakeDamage;
    public Detection Detection {get; private set; }
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

    #region callbacks and core
    protected override void Start() {
        base.Start();
        Detection = this.GetComponent<Detection>();
        agent = this.GetComponent<NavMeshAgent>();
        setupAnimationHashes();
        //if (patrolWaypoints.Any()) NextWaypointLocation = patrolWaypoints[0].transform.position; //set first patrol waypoint
        //SetStateDriver(new PatrolState(this, animator, agent));
    }
    
    public override void TakeDamage(float damage) {
        base.TakeDamage(damage);
        //release an event indicating x has taken damage
        onTakeDamage.Invoke(this);
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

    #region stealthstuff
    public bool LOSOnPlayer() {
        return Detection.VisibleTargets.Count != 0;
    }
    
    public BTStatus VerifyStealth() { //verify if stealth is valid
        //todo: also check for transitions DIRECTLY to aggro
        return GlobalState == AIGlobalState.AGGRO? BTStatus.FAILURE : BTStatus.RUNNING;
    }

    public void SetNextPatrolDestination() { 
        currPatrolIndex = (currPatrolIndex + 1) % patrolWaypoints.Count;
        NextWaypointLocation = patrolWaypoints[currPatrolIndex].transform.position;
    }
    #endregion

    #region combat
    /*
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
    */
    private IEnumerator ChaseCoroutine(){
        while (true){
            agent.SetDestination(target.transform.position);
            yield return null;
        }
    }
    #endregion
    
}