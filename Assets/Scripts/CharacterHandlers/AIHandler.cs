using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public enum GlobalState { UNAGGRO, AGGRO, DEAD }; //determines ai state tree area - has to be manually set (Cause why not ruin code with even more dependencies)

[RequireComponent(typeof(Collider))] //cause hit registry requires colliders
public class AIHandler : CharacterHandler {    

    [Header("AI Core Components/SOs")]
    public bool startAsAggro = false; //is tha ai patrolling, or is the ai fighting

    //stealth stuff
    [Header("Stealth stuff")]
    public List<PatrolWaypoint> patrolWaypoints; 
    public float idleTimeAtWaypoint; //how long ai stays at each patrol waypoint
    public float AIGlobalStateCheckRange = 30f; //range ai can sense other AI and their states (should be tested)

    [Header("Combat Stuff")] //combat rings determine AI behavior (more below)
    public float tooFarFromPlayerDistance; 
    public float backAwayDistance;
    public float shoveDistance;
    public float minWaitBetweenAttacks;
    public float defenseRefresh;

    [Header("debug")]
    public Image stealthBar; //for debug i think
    public TextMeshProUGUI AIstate; 

    //private stuff
    public PlayerHandler TargetPlayer {get; private set;}  //always have a reference to the player
    protected NavMeshAgent agent; //all AI stuff
    protected AIThinkState thinkState;  //different from GenericState, this is an FSM for ai specific stuff
    public float CurrSpotTimerThreshold {get; private set; } //aka time it takes when ai spots player to transition to full aggro - depends on the player's stance 
    public Detection Detection {get; private set; } //detection rings (for stealth)
    public GlobalState GlobalState {get; set; } = GlobalState.UNAGGRO; //spawn/start as aggro (todo unless otherwise stated)
    public Vector3 NextWaypointLocation {get; private set;}  //the next waypoint the ai goes to
    public Quaternion NextWaypointRotation {get; set;} //manually change where the ai faces at each waypoint
    private LayerMask AIMask; //for physics identification on other AI
    private float recentHits;

    //static
    public static List<AIHandler> CombatAI {get; set;}


    #region callbacks
    protected override void Start() {
        base.Start(); //all characterhandler stuff
        
        Detection = this.GetComponent<Detection>();
        agent = this.GetComponent<NavMeshAgent>();
      
        //auto find player finally
        try { TargetPlayer = FindObjectOfType<PlayerHandler>(); } catch { Debug.LogWarning("WHERE THE PLAYER AT FOOL"); }

        //layers are currently hard coded my b
        AIMask = LayerMask.GetMask("Enemy");
        if(AIMask == -1) Debug.LogWarning("AI MASK NOT SET PROPERLY");
        if(gameObject.layer != LayerMask.NameToLayer("Enemy")) Debug.LogWarning ("layer should be set to Enemy, not " + LayerMask.LayerToName(gameObject.layer));

        //AI should always be in default combat state - read more in generic state folder
        genericState = new DefaultCombatState(this, animator); //todo temp probably

        //event stuff - the player changing stances triggers an event that effects the AI detection time
        CurrSpotTimerThreshold = (TargetPlayer.characterdata as PlayerData).detectionTime;
        TargetPlayer.OnStanceChangeTimer += SpotTimerChange;

        if(CombatAI == null){
            CombatAI = new List<AIHandler>();
        }
        
        StartingCondition();
        
        
    }  
    #endregion

    //invoked as a event in playerHandler
    public void SpotTimerChange(float spotModify){
        CurrSpotTimerThreshold = spotModify;
    }

    //deals with starting as aggro or not
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

        if(thinkState != null) AIstate.SetText(thinkState.ToString()); //only for debug
        //Debug.Log(GlobalState);


        animator.SetBool(Animator.StringToHash("Combat"), GlobalState == GlobalState.AGGRO); //this shouldnt be here
        
    }

    void LateUpdate() {
        HandleMovementAnim(); 
    }

    #region core
    Vector3 preVelocity, velVel, currVelocity;
    private void HandleMovementAnim() { //this tells the animation controller to blend between forward/backward/left/right based off of character velocity
        currVelocity = Vector3.SmoothDamp(preVelocity, agent.velocity, ref velVel, .12f);
        preVelocity = currVelocity;

        Vector3 localDir = transform.InverseTransformDirection(currVelocity).normalized;
        
        //Debug.Log(currVelocity.magnitude);

        float weight = Mathf.InverseLerp(0, agent.speed, currVelocity.magnitude);
        animator.SetFloat(Animator.StringToHash("XMove"), localDir.x * weight);
        animator.SetFloat(Animator.StringToHash("ZMove"), localDir.z * weight);
        animator.SetFloat(Animator.StringToHash("Speed"), weight);



    }

    //override cause AI is allowed to enter deathstate, and player isnt (for now)
    protected override bool TakeDamageAndCheckDeath(float damage, bool isStaggerable, CharacterHandler attacker) {
        recentHits += 1;
        
        if (base.TakeDamageAndCheckDeath(damage, isStaggerable, attacker)){
            SetStateDriver(new DeathState(this, animator)); 
            return true;
        } else {
            if(GlobalState != GlobalState.AGGRO) PivotToAggro();
            return false;
        }
        
    }

    //going from unaggro -> aggro
    public void PivotToAggro() {
        GlobalState = GlobalState.AGGRO;
        animator.SetBool(Animator.StringToHash("Combat"), true); //todo: to be put in separate class    

        //add to static list of all combat AI
        CombatAI.Add(this);

        StartCoroutine(OffenseScheduler());
        StartCoroutine(CirclingAssistant());
        StartCoroutine(DefenseRefreshing());
        SetStateDriver(new DefaultAIAggroState(this, animator, agent));
    }

    #endregion

    #region AIFSM
    //upon combat start, begin actively calculating probabillity
    //this is an arbritrary system we came up with trying to reverse engineer other games' AI
    public bool CanOffend {get; set; } = false;
    public bool CirclingIndicator {get; set; } = false;
    //private bool offenseCooldown = false;
    private IEnumerator OffenseScheduler() {
        //default offense chance
        while (GlobalState != GlobalState.DEAD) {
            CanOffend = false;

            //todo list:
                //allow manipulation of this value:
                    //if player misses, speed this up
                    //dodges within the hot zone radius
                    //if player is healing

            //cooldown between each attack
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

            //the attack is over
        }
        yield break;
    }


    private IEnumerator CirclingAssistant() {
        while (GlobalState != GlobalState.DEAD) {
            CirclingIndicator = false;
            yield return new WaitForSeconds(Random.Range(6-1f, 6+1f));
            int currentCircling = 0;
            foreach(AIHandler ai in CombatAI){
                if(ai.thinkState is CirclingState) currentCircling++;
            }
            //Basically took the code for the offense scheduler, this code is largely for the purposes of limiting the number of enemies that can be
            //circling the player at a time, currently, we are limiting it to 3 max enemies that can circle the player at once.
            while (currentCircling > 3) {
                currentCircling = 0;
                foreach(AIHandler ai in CombatAI){
                    if(ai.thinkState is CirclingState) currentCircling++;
                }
                yield return new WaitForSeconds(1f);
            }

           CirclingIndicator = true;
           yield return new WaitUntil(() => thinkState is CirclingState);
           yield return new WaitWhile(() => thinkState is CirclingState);
        }
        yield break;
    }

    //aifsm helpers
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

    //note: all the functions below are invoked in the ThinkCycle class
    //these serve to tell what the AI actually does, the ThinkCycle class determines when they should be used

    #region stealthstuff

    public BTStatus VerifyStealth() {
        if(GlobalState != GlobalState.UNAGGRO) return BTStatus.FAILURE; 

        //cast a sphere of AIGlobalStateCheckRange size, if any AI in that sphere is Aggro, turn into aggro as well
        Collider[] aiInRange = Physics.OverlapSphere(transform.position, AIGlobalStateCheckRange, AIMask);
            foreach(Collider col in aiInRange) {
            if(col.GetComponent<AIHandler>().GlobalState == GlobalState.AGGRO){ //note: getcomponent in loops are bad cause slow
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

        // return TargetPlayer.genericState is AttackState //player is attacking
        // && (TargetPlayer.transform.position - transform.position).sqrMagnitude < TargetPlayer.weapon.Attacks.First().range * TargetPlayer.weapon.Attacks.First().range //player is close enough (temp)
        // && !(genericState is AttackState) //im not already mid attack  
        // && canDefend()
        // && Stamina > 0;     
        if (TargetPlayer.genericState is AttackState //player is attacking
        && (TargetPlayer.transform.position - transform.position).sqrMagnitude < TargetPlayer.weapon.Attacks.First().range * TargetPlayer.weapon.Attacks.First().range //player is close enough (temp)
        && !(genericState is AttackState) //im not already mid attack  
        && canDefend()
        && Stamina > 0){
            recentHits += 1;
            return true;
        }
        else return false;
    }

    private bool canDefend(){
        //probability is 0.33 per hit, so first hit has 1 chance of blocking, second is 0.66, third is 0.33, fourth is 0
        double failBlock = recentHits * 0.33;
        return Random.Range(0.001f, 1.0f) > failBlock;
    }


    private IEnumerator DefenseRefreshing() {
        //default offense chance
        //There exists a new parameter called defenseRefresh, basically, it counts from the first hit/defense, and reduces the recent hits to zero
        //The higher recent hits is, the less likely the AI will be able to block
        while (GlobalState != GlobalState.DEAD) {
            yield return new WaitForSeconds(0.1f);
            if (recentHits > 0) {
                yield return new WaitForSeconds(defenseRefresh);
                recentHits = 0;
            }
        }
        yield break;
    }

    //returns if player is too far
    public bool CloseDistanceConditional() => (TargetPlayer.transform.position - transform.position).sqrMagnitude > tooFarFromPlayerDistance * tooFarFromPlayerDistance;

    //determine if ai should charge
    public bool WorthCharging() => CloseDistanceConditional() && CanOffend || thinkState is ChargeState;
    

    public bool SpacingConditional() {
        //if i am currently spacing
        return false;
        
    }
//canOffend initiates it, thinkState check ensures it goes through
    public bool OffenseConditional() => CanOffend || thinkState is OffenseState;

    public bool CirclingConditional() => CirclingIndicator || thinkState is CirclingState;


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

    public BTStatus ChargingTask(){
        if(!(thinkState is ChargeState)){
            SetStateDriver(new ChargeState(this, animator, agent));
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
        // if(!(thinkState is BackAwayState)) {
        //     if(CanOffend) {
        //         SetStateDriver(new OffenseState(this, animator, agent));
        //     } else {
        //         bool f = Random.Range(0, 1) == 0;
        //         if(f) SetStateDriver(new BackAwayState(this, animator, agent));
        //     }
        // }

        if(!(thinkState is BackAwayState)) {
            SetStateDriver(new BackAwayState(this, animator, agent));
            
        }
        
        return BTStatus.RUNNING;
    }

    public BTStatus CirclingTask() {
        if(!(thinkState is CirclingState)) { 
            SetStateDriver(new CirclingState(this, animator, agent)); //todo attack move selection
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
