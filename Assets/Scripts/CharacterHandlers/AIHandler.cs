using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public enum GlobalState { UNAGGRO, AGGRO, DEAD }; //determines ai state tree area

[RequireComponent(typeof(Collider))] //cause hit registry requires colliders
public class AIHandler : CharacterHandler {    

    [Header("AI Core Components/SOs")]
    public bool startAsAggro = false;

    //stealth stuff
    [Header("Stealth stuff")]
    public Image stealthBar;
    public List<PatrolWaypoint> patrolWaypoints; //where ai walks
    public float idleTimeAtWaypoint; //how long ai stays at each patrol waypoint
    public float AIGlobalStateCheckRange = 30f; //range ai can sense other AI and their states

    [Header("Combat Stuff")]
    public float tooFarFromPlayerDistance;
    public float backAwayDistance;
    public float shoveDistance;
    public float minWaitBetweenAttacks;

    [Header("debug")]
    public TextMeshProUGUI AIstate; 

    //private stuff
    public PlayerHandler TargetPlayer {get; private set;} 
    protected NavMeshAgent agent;
    protected AIThinkState thinkState; 
    public float CurrSpotTimerThreshold {get; private set; }
    public Detection Detection {get; private set; }
    public GlobalState GlobalState {get; set; } = GlobalState.UNAGGRO; //spawn/start as aggro (todo unless otherwise stated)
    public Vector3 NextWaypointLocation {get; private set;} 
    public Quaternion NextWaypointRotation {get; set;}
    private LayerMask AIMask;

    //static
    public static List<AIHandler> CombatAI {get; set;}


    #region callbacks
    protected override void Start() {
        base.Start(); //all character stuff
        Detection = this.GetComponent<Detection>();
        agent = this.GetComponent<NavMeshAgent>();
      
        //auto find player finally
        try { TargetPlayer = FindObjectOfType<PlayerHandler>(); } catch { Debug.LogWarning("WHERE THE PLAYER AT FOOL"); }

        AIMask = LayerMask.GetMask("Enemy");
        if(AIMask == -1) Debug.LogWarning("AI MASK NOT SET PROPERLY");
        if(gameObject.layer != LayerMask.NameToLayer("Enemy")) Debug.LogWarning ("layer should be set to Enemy, not " + LayerMask.LayerToName(gameObject.layer));

        //AI should always be in default combat state
        genericState = new DefaultCombatState(this, animator); //todo temp probably

        //event stuff
        CurrSpotTimerThreshold = (TargetPlayer.characterdata as PlayerData).detectionTime;
        TargetPlayer.OnStanceChangeTimer += SpotTimerChange;

        CombatAI = new List<AIHandler>();

        StartingCondition();
        
        
    }  
    #endregion

    public void SpotTimerChange(float spotModify){
        CurrSpotTimerThreshold = spotModify;
    }

    private void StartingCondition(){
        if(startAsAggro) {
            PivotToAggro();

        } else {
            GlobalState = GlobalState.UNAGGRO; // temp
            if (patrolWaypoints.Count != 0) {
                NextWaypointLocation = patrolWaypoints[0].transform.position; //set first patrol waypoint
                NextWaypointRotation = patrolWaypoints[0].transform.rotation;
            }else {
                Debug.Log("this ai dont have waypoint for patrols, should be at least one probs");
            }
            
            SetStateDriver(new PatrolState(this, animator, agent));
        }
    }

    protected override void Update(){
        base.Update();

        if(thinkState != null) AIstate.SetText(thinkState.ToString());
        //Debug.Log(GlobalState);


        animator.SetBool(Animator.StringToHash("Combat"), GlobalState == GlobalState.AGGRO);
        
    }

    void LateUpdate() {
        HandleMovementAnim();
    }

    #region core
    Vector3 preVelocity, velVel, currVelocity;
    private void HandleMovementAnim() {
        currVelocity = Vector3.SmoothDamp(preVelocity, agent.velocity, ref velVel, .12f);
        preVelocity = currVelocity;

        Vector3 localDir = transform.InverseTransformDirection(currVelocity).normalized;

        //Debug.Log(currVelocity.magnitude);

        float weight = Mathf.InverseLerp(0, agent.speed, currVelocity.magnitude);
        animator.SetFloat(Animator.StringToHash("XMove"), localDir.x * weight);
        animator.SetFloat(Animator.StringToHash("ZMove"), localDir.z * weight);
        animator.SetFloat(Animator.StringToHash("Speed"), weight);



    }

    protected override bool TakeDamageAndCheckDeath(float damage, bool isStaggerable, CharacterHandler attacker) {
        if (base.TakeDamageAndCheckDeath(damage, isStaggerable, attacker)){
            SetStateDriver(new DeathState(this, animator)); 
            return true;
        } else {
            if(GlobalState != GlobalState.AGGRO) PivotToAggro();
            return false;
        }

    }

    public void PivotToAggro() {
        GlobalState = GlobalState.AGGRO;
        animator.SetBool(Animator.StringToHash("Combat"), true); //todo: to be put in separate class    

        //add to static list of all combat AI
        CombatAI.Add(this);

        StartCoroutine(OffenseScheduler());
        SetStateDriver(new DefaultAIAggroState(this, animator, agent));
    }


    //upon combat start, begin actively calculating probabillity
    public bool CanOffend {get; set; } = false;
    //private bool offenseCooldown = false;
    private IEnumerator OffenseScheduler() {
        //default offense chance
        while (GlobalState != GlobalState.DEAD) {
            CanOffend = false;

            //cooldown between each attack
                //allow manipulation of this value:
                    //if player misses, speed this up
                    //dodges within the hot zone radius
                    //if player is healing
            yield return new WaitForSeconds(Random.Range(minWaitBetweenAttacks-1f, minWaitBetweenAttacks+1f));

            //check once if there arent too many ai attacking
            int offenseAI = 0;
            foreach(AIHandler ai in CombatAI){
                if(ai.thinkState is OffenseState) offenseAI++;
            }

            //if too many ai attacking, loop until theres a spot
            while (offenseAI > 3) {
                offenseAI = 0;
                foreach(AIHandler ai in CombatAI){
                    if(ai.thinkState is OffenseState) offenseAI++;
                }
                yield return new WaitForSeconds(1f); //wait between each check until an attack is available
            }


           //as of this point the ai is allowed to attack
           CanOffend = true;
           //wait until the offense begins, and then once it does, wait till it finishes
           yield return new WaitUntil(() => thinkState is OffenseState);
           yield return new WaitWhile(() => thinkState is OffenseState);

        }
        yield break;
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

    public BTStatus VerifyStealth() {
        if(GlobalState != GlobalState.UNAGGRO) return BTStatus.FAILURE; 

        //cast a sphere, if any AI in that sphere is Aggro, turn into aggro as well
        //require targetMask
        Collider[] aiInRange = Physics.OverlapSphere(transform.position, AIGlobalStateCheckRange, AIMask);
            foreach(Collider col in aiInRange) {
            if(col.GetComponent<AIHandler>().GlobalState == GlobalState.AGGRO){
                PivotToAggro();
                return BTStatus.FAILURE;
            }
        }

        return BTStatus.SUCCESS;
    }

    //check if ai has line of sight on player
    public bool LOSOnPlayer() => Detection.VisibleTargets.Count != 0;
    
    
    //move to next patrol point
    private int currPatrolIndex;
    public void SetNextPatrolDestination() { 
        currPatrolIndex = (currPatrolIndex + 1) % patrolWaypoints.Count;
        NextWaypointLocation = patrolWaypoints[currPatrolIndex].transform.position;
        NextWaypointRotation = patrolWaypoints[currPatrolIndex].transform.rotation;
    }
    #endregion
    
    #region combatstuff
    //deals with AI BT if combat is still a viable option
    public bool VerifyCombatCapable() { 
        //if not combat capable (either dead or pu55y), fail
        //otherwise, run OVERRIDE method from child class
     //   Debug.Log("combat condi");
        return GlobalState != GlobalState.DEAD;

    }

    public bool StaggerCheckConditional() => genericState is StaggerState;
    

    public bool DefenceConditional(){       //  Debug.Log("defense condi");

        return TargetPlayer.genericState is AttackState //player is attacking
        && (TargetPlayer.transform.position - transform.position).sqrMagnitude < TargetPlayer.weapon.Attacks.First().range * TargetPlayer.weapon.Attacks.First().range //player is close enough (temp)
        && !(genericState is AttackState) //im not already mid attack  
        && Stamina > 0;     

    }

    //returns if player is too far
    public bool CloseDistanceConditional() => (TargetPlayer.transform.position - transform.position).sqrMagnitude > tooFarFromPlayerDistance * tooFarFromPlayerDistance;

    public bool SpacingConditional() {
        //if i am currently spacing
        return false;
        
    }
//canOffend initiates it, thinkState check ensures it goes through
    public bool OffenseConditional() => CanOffend || thinkState is OffenseState;

    public bool BackAwayConditional() { //if too close AND CAN back away
        NavMeshHit hit;

        // Debug.Log(agent.FindClosestEdge(out hit));

        return (TargetPlayer.transform.position - transform.position).sqrMagnitude <= backAwayDistance * backAwayDistance
                && agent.FindClosestEdge(out hit); //NavMesh.FindClosestEdge(transform.position + (transform.position - TargetPlayer.transform.position), out hit, NavMesh.AllAreas);


    }

    //returns if player is too close
    public bool InstantShoveConditional() => (TargetPlayer.transform.position - transform.position).sqrMagnitude <= shoveDistance * shoveDistance; 

    public BTStatus StaggerTask() {   

        //Debug.Log("idk what stagger");
        return BTStatus.RUNNING;
    }

    public BTStatus DefenseTask() { //Debug.Log("defend task");
        if(!(thinkState is DefenseState)) { //if im not already defending
            SetStateDriver(new DefenseState(this, animator, agent));
        }
        return BTStatus.RUNNING;
    }

    public BTStatus ChasingTask() { //Debug.Log("chase task");
        //regular chase OR charge attack

        if(!(thinkState is ChaseState)) { 
            SetStateDriver(new ChaseState(this, animator, agent));
        }
        return BTStatus.RUNNING;
    }

    public BTStatus OffenseTask() {// Debug.Log("offense task");

        //begin an attack if neccesary
        if(!(thinkState is OffenseState)) { 
            SetStateDriver(new OffenseState(this, animator, agent, RandomMoveSelection())); //todo attack move selection
        }

        return BTStatus.RUNNING;
    }

    private MeleeMove RandomMoveSelection(){
        return weapon.Attacks[Random.Range(0, weapon.Attacks.Count)];
    }

    public BTStatus BackAwayTask() {// Debug.Log("back away");\

       // Debug.Log("back away tasking");
        //implication: cannot be in offense state if this area is reached
        if(!(thinkState is BackAwayState)) {
            if(CanOffend) {
                SetStateDriver(new OffenseState(this, animator, agent));
            } else {
                bool f = Random.Range(0, 1) == 0;
                if(f) SetStateDriver(new BackAwayState(this, animator, agent));
            }
        }
        
        return BTStatus.RUNNING;
    }

    public BTStatus ShovingTask() { //Debug.Log("shove task");
        if(!(thinkState is ShoveState)) { 
            SetStateDriver(new ShoveState(this, animator, agent));
        }
        return BTStatus.RUNNING;
    }

    // public BTStatus SpacingTask() {
    //     if(!(thinkState is SpacingState)) { 
    //         SetStateDriver(new SpacingState(this, animator, agent));
    //     }
    //     return BTStatus.RUNNING;
    // }


    //memthods used by think state behavior
    public IEnumerator ChasePlayer(float stopRange) {
        agent.SetDestination(TargetPlayer.transform.position);
        while(stopRange < agent.remainingDistance || agent.pathPending){
            agent.SetDestination(TargetPlayer.transform.position);
            yield return new WaitForSeconds(.2f);
        }
        agent.SetDestination(transform.position);
    }

    public IEnumerator SpaceFromPlayer(float distanceFromPlayer){
        NavMeshHit hit;

        // Vector3 backAwayMainVec = (transform.position - targetPlayer.transform.position).normalized * (backAwayDistance + 15f);
        // Vector3 movePos = (transform.position + backAwayMainVec) - (transform.position + (transform.position - targetPlayer.transform.position));
        NavMesh.SamplePosition(transform.position + ((transform.position - TargetPlayer.transform.position).normalized * (backAwayDistance + 10f - Vector3.Distance(transform.position, TargetPlayer.transform.position))), out hit, distanceFromPlayer, NavMesh.AllAreas);
        agent.SetDestination(hit.position);
        
        while(((NavMesh.SamplePosition(transform.position + ((transform.position - TargetPlayer.transform.position).normalized * (backAwayDistance + 10f - Vector3.Distance(transform.position, TargetPlayer.transform.position))), out hit, distanceFromPlayer, NavMesh.AllAreas))
                || (agent.FindClosestEdge(out hit)) //(NavMesh.FindClosestEdge(transform.position + ((transform.position - TargetPlayer.transform.position).normalized * (backAwayDistance + 10f - Vector3.Distance(transform.position, TargetPlayer.transform.position))), out hit, NavMesh.AllAreas))
              ) && (agent.stoppingDistance < agent.remainingDistance || agent.pathPending) 
                ) {

            agent.SetDestination(hit.position);
            // backAwayMainVec = (transform.position - targetPlayer.transform.position).normalized * (backAwayDistance + 15f);
            // movePos = (transform.position + backAwayMainVec) - (transform.position + (transform.position - targetPlayer.transform.position));
            yield return new WaitForSeconds(.1f);
        }

      //  Debug.Log(hit.position);
    }

    public IEnumerator FacePlayer() {
        while (GlobalState != GlobalState.DEAD) {
            transform.LookAt(new Vector3(TargetPlayer.transform.position.x, this.transform.position.y, TargetPlayer.transform.position.z));
            yield return null;
        }
    }

    #endregion

    #region self preservation
    public BTStatus SelfPreserve() {
        return BTStatus.RUNNING;
        //if character is dead, fail
    }

    #endregion
    
}   