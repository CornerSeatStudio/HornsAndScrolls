using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using TMPro;

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
    public float tooFarFromPlayerDistance;
    public float shoveDistance;

    [Header("debug")]
    public TextMeshProUGUI AIstate; 

    //private stuff
    protected NavMeshAgent agent;
    protected AIThinkState thinkState; 
    public Detection Detection {get; private set; }
    public GlobalState GlobalState {get; set; } = GlobalState.UNAGGRO; //spawn/start as aggro (todo unless otherwise stated)
    public Vector3 NextWaypointLocation {get; private set;} 

    #region callbacks
    protected override void Start() {
        base.Start(); //all character stuff
        Detection = this.GetComponent<Detection>();
        agent = this.GetComponent<NavMeshAgent>();
        if (patrolWaypoints.Any()) NextWaypointLocation = patrolWaypoints[0].transform.position; //set first patrol waypoint
        SetStateDriver(new PatrolState(this, animator, agent));
      //  thinkState = new DefaultAIAggroState(this, animator, agent);
        genericState = new DefaultCombatState(this, animator); //todo temp probably
       // GlobalState = GlobalState.AGGRO; // temp
    }  
    #endregion

    protected override void Update(){
        base.Update();
        if(thinkState != null) AIstate.SetText(thinkState.ToString());
        //Debug.Log(GlobalState);


        animator.SetBool(Animator.StringToHash("IsGlobalAggroState"), GlobalState == GlobalState.AGGRO);
        if(GlobalState == GlobalState.AGGRO) HandleCombatMovementAnim();
        
    }

    #region core

    private void HandleCombatMovementAnim() {
        CalculateVelocity();

       // Debug.Log(velocity);

        Vector3 localDir = transform.InverseTransformDirection(velocity).normalized;
        animator.SetFloat(Animator.StringToHash("XCombatMove"), localDir.x);
        animator.SetFloat(Animator.StringToHash("ZCombatMove"), localDir.z);
        animator.SetBool(Animator.StringToHash("CombatWalking"), (Mathf.Abs(localDir.x) > 0.1f) || (Mathf.Abs(localDir.z) > 0.1f));


    }
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
    public void SetStateDriver(AIThinkState state) { 
        StartCoroutine(SetState(state));
    }

    private IEnumerator SetState(AIThinkState state) {
        if(thinkState != null) yield return StartCoroutine(thinkState.OnStateExit());
        thinkState = state;
        yield return StartCoroutine(thinkState.OnStateEnter());
    }

    #endregion

    #region stealthstuff
    //verify if stealth is valid
    public BTStatus VerifyStealth() {
        if(GlobalState != GlobalState.UNAGGRO) return BTStatus.FAILURE; 

        //cast a sphere, if any AI in that sphere is Aggro, turn into aggro as well
        Collider[] aiInRange = Physics.OverlapSphere(transform.position, AIGlobalStateCheckRange, Detection.obstacleMask);
            foreach(Collider col in aiInRange) {
            AIHandler proximateAI = col.GetComponent<AIHandler>();
            if(proximateAI.GlobalState == GlobalState.AGGRO){
                GlobalState = GlobalState.AGGRO;
                animator.SetBool(Animator.StringToHash("IsGlobalAggroState"), true); //todo: to be put in separate class
                
                layerWeightRoutine = LayerWeightDriver(1, 0, 1, .3f);
                StartCoroutine(layerWeightRoutine);
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
     //   Debug.Log("combat condi");
        return GlobalState == GlobalState.DEAD;

    }

    public bool StaggerCheckConditional() {     //    Debug.Log("stag condi");
        return genericState is StaggerState;
    }

    public bool DefenceConditional(){       //  Debug.Log("defense condi");

        return Detection.VisibleTargets.Any() //if i can see player
        && Detection.VisibleTargets[0].GetComponent<CharacterHandler>().genericState is AttackState //player is attacking
        && Detection.VisibleTargets[0].GetComponent<CharacterHandler>().FindTarget(((Detection.VisibleTargets[0].GetComponent<CharacterHandler>().genericState) as AttackState).chosenMove) == this //player is attacking me in particular
        && !(genericState is AttackState)
        && Stamina > 0; //im not already mid attack      
    }

    public bool CloseDistanceConditional() { //returns if player is too far
           //    Debug.Log("far condi");

        return (targetPlayer.transform.position - transform.position).sqrMagnitude > tooFarFromPlayerDistance * tooFarFromPlayerDistance;
    }

    public bool InstantShoveConditional() { //returns if player is too close
               // Debug.Log("close condi");

        return (targetPlayer.transform.position - transform.position).sqrMagnitude <= shoveDistance * shoveDistance;
    }

    public BTStatus StaggerTask() {   

      //  Debug.Log("idk what stagger");
        return BTStatus.RUNNING;
    }

    public BTStatus DefenseTask() {// Debug.Log("defend task");
        // if(!(thinkState is DefenseState)) { //if im not already defending
        //     SetStateDriver(new DefenseState(this, animator, agent));
        // }
        return BTStatus.RUNNING;
    }

    public BTStatus ChasingTask() {// Debug.Log("chase task");
        if(!(thinkState is ChaseState)) { 
            SetStateDriver(new ChaseState(this, animator, agent));
        }
        return BTStatus.RUNNING;
    }

    public BTStatus ShovingTask() { //Debug.Log("shove task");
        if(!(thinkState is ShoveState)) { 
            SetStateDriver(new ShoveState(this, animator, agent));
        }
        return BTStatus.RUNNING;
    }

    public BTStatus OffenseTask() { //Debug.Log("offense task");
        if(!(thinkState is OffenseState)) { 
            SetStateDriver(new OffenseState(this, animator, agent)); //todo attack move selection
        }
        return BTStatus.RUNNING;
    }

    


    //memthods used by think state behavior
    public IEnumerator ChasePlayer(float stopRange) {
        agent.SetDestination(targetPlayer.transform.position);
        while(stopRange < agent.remainingDistance || agent.pathPending){
            agent.SetDestination(targetPlayer.transform.position);
            yield return new WaitForSeconds(.2f);
        }
        agent.SetDestination(transform.position);
    }

    public IEnumerator SpaceFromPlayer(float distanceFromPlayer){
        

        NavMeshHit hit;
        //if i found a position away from player
        if(NavMesh.SamplePosition(transform.position + (transform.position - targetPlayer.transform.position), out hit, distanceFromPlayer, NavMesh.AllAreas)){
           // Debug.Log("sampled location");
        } else if (NavMesh.FindClosestEdge(transform.position + (transform.position - targetPlayer.transform.position), out hit, NavMesh.AllAreas)){
          //  Debug.Log("sample failed, found closest edge");
        } else {
           // Debug.Log("nowhere tf");
        }
        
        // Debug.Log("ai: " + transform.position);
        // Debug.Log("pl: " + targetPlayer.transform.position);
        // Debug.Log("new pos: " + (transform.position - targetPlayer.transform.position));

        agent.SetDestination(hit.position);
        yield return new WaitForSeconds(5f); //back off for this long (or until chase range kicks in?)
        agent.SetDestination(transform.position);
        
    }

    #endregion

    #region self preservation
    public BTStatus SelfPreserve() {
        return BTStatus.RUNNING;
        //if character is dead, fail
    }

    #endregion
    
}   