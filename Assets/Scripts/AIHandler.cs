using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum WeaponType { MELEE, RANGED };
public enum AIState { IDLE, PATROL, SLEEPING, COMBAT, DEAD }; 
public enum CombatSlot {GUARANTEE, EVICTION, IN, OUT};

[System.Serializable] public class AISpecificEvent : UnityEvent<AIHandler>{}

public class AIHandler : CharacterHandler {    

    //core
    public CharacterHandler target; 
    public WeaponData weapon;
    [SerializeField] public AIState AIState {get; set; } = AIState.IDLE;
    private List<AIHandler> proximateAI;
    public float Priority { get; set; }
    public AISpecificEvent onTakeDamage;


    //stealth stuff
    public float spotTimerThreshold;
    public float spotTimerDivisions;
    private float spotTimer;

    //combat stuff
    public int combatPocket;
    public CombatSlot CombatSlot {get; set;} = CombatSlot.OUT;

    //nav stuff
    private NavMeshAgent agent;
    private bool chaseInitializer = true;

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

    public void Update() {
        Debug.Log(CombatSlot);
    }

    //general behaviors
    //non combat tree
    public bool ValidifyCombatState() {return true; }//if this succeeds, go to combat routine, otherwise, continue stealth routine 
    public bool PlayerInLOS() { return hitDetection.VisibleTargets.Count != 0;}
    public BTStatus MaintainLOSWhileStationary() {return BTStatus.RUNNING;} //guarrantees both look direction and posiion remain constant
    public BTStatus IncrementSpotTimer(float thinkDelay) {
        spotTimer += thinkDelay;
        return BTStatus.SUCCESS;
    }
    public BTStatus DecrementSpotTimer(float thinkDelay) {
        if (spotTimerThreshold % spotTimerDivisions != 0) spotTimer -= thinkDelay;
        return BTStatus.SUCCESS;
    }
    public BTStatus ExecutePatrol() {return BTStatus.RUNNING;}
    public BTStatus Idle() {return BTStatus.SUCCESS;}

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

    bool localSwingWillyFlag = true;
    public BTStatus SwingWilly() {

        if(hitDetection.InMeleeRoutine) { 
            return BTStatus.RUNNING;
        } else if (localSwingWillyFlag) {
            agent.isStopped = true;
            StartCoroutine(hitDetection.InitAttack(weapon.startup, weapon.endlag, weapon.damage));
            localSwingWillyFlag = false;
            return BTStatus.RUNNING;
        } 
        else {           
            localSwingWillyFlag = true;
            agent.isStopped = false;
            return BTStatus.SUCCESS;
        }
    }

    public BTStatus SpaceFromTarget() {return BTStatus.RUNNING;}
    public BTStatus BopFoolWithArrow() {return BTStatus.RUNNING;}
 
    private IEnumerator ChaseCoroutine(){
        while (true){
            agent.SetDestination(target.transform.position);
            yield return null;
        }
    }
}