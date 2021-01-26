using System.Collections;
using System;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : AIThinkState {
    private IEnumerator idleCoroutine, LOSCoroutine;
    
    public IdleState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}
    
    public override IEnumerator OnStateEnter() { //Debug.Log("enteredIdle, timer: " + character.idleTimeAtWaypoint);
        
        idleCoroutine = OrientToWaypointRotation();
        yield return character.StartCoroutine(idleCoroutine);
        idleCoroutine = IdleTimer(character.idleTimeAtWaypoint);
        yield return character.StartCoroutine(idleCoroutine);
    }

    private IEnumerator OrientToWaypointRotation(){
        Quaternion newRotation = character.NextWaypointRotation;
        float timeCount = 0;
        while(timeCount < 1){
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, newRotation, timeCount);
            timeCount += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator LOSOnPlayerCheck() { //once spotted, go to investigate
        //Debug.Log("checking for player");
        yield return new WaitUntil(() => character.LOSOnPlayer());
        character.SetStateDriver(new InvestigationState(character, animator, agent));
    }

    private IEnumerator IdleTimer(float idleTimeAtWaypoint) { //Debug.Log("enteredIdleTimer");
        LOSCoroutine = LOSOnPlayerCheck();
        character.StartCoroutine(LOSCoroutine);
        yield return new WaitForSeconds(idleTimeAtWaypoint);
        //once done, switch to Patrol
        character.SetStateDriver(new PatrolState(character, animator, agent));
    }

    public override IEnumerator OnStateExit() { //Debug.Log("exitedIdle");
        if(idleCoroutine != null) character.StopCoroutine(idleCoroutine); //stop coroutine
        if(LOSCoroutine != null) character.StopCoroutine(LOSCoroutine);
        character.SetNextPatrolDestination();//only change next patrol destination AFTER idling at current one
        yield return null;
    }
}

public class PatrolState : AIThinkState {
    private IEnumerator moveToLocationCoroutine, LOSCoroutine;

    public PatrolState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}

    public override IEnumerator OnStateEnter() { //Debug.Log("enteredPatrol");
        if(character.NextWaypointLocation == null) { Debug.LogWarning("no waypoints on some cunt"); character.SetStateDriver(new IdleState(character, animator, agent)); };
        
        moveToLocationCoroutine = MoveToLocation(character.NextWaypointLocation);
        yield return character.StartCoroutine(moveToLocationCoroutine);
    }    

    private IEnumerator MoveToLocation(Vector3 location){ //Debug.Log("enteredMove");/
        LOSCoroutine = LOSOnPlayerCheck();
        character.StartCoroutine(LOSCoroutine);
        agent.SetDestination(location);
        yield return new WaitUntil(() => !agent.pathPending && agent.stoppingDistance > agent.remainingDistance);
        character.SetStateDriver(new IdleState(character, animator, agent));
    }

    private IEnumerator LOSOnPlayerCheck() { //once spotted, go to investigate
        //Debug.Log("checking for player");
        yield return new WaitUntil(() => character.LOSOnPlayer());
        character.SetStateDriver(new InvestigationState(character, animator, agent));
    }

    public override IEnumerator OnStateExit() { //Debug.Log("exitedPatrol");
        if(LOSCoroutine != null) character.StopCoroutine(LOSCoroutine);        
        if(moveToLocationCoroutine != null) character.StopCoroutine(moveToLocationCoroutine); //stop coroutine
        yield return null;
    }

}

public class InvestigationState : AIThinkState {
    public float CurrInvestigationTimer {get; private set;}
    private Vector3 lastSeenPlayerLocation;
    private IEnumerator timerCoroutine, investigationCoroutine, lookCoroutine;
    private bool isDecreasingDetection, ringUp = false;

    public InvestigationState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}

    public override IEnumerator OnStateEnter() {
        //Debug.Log("entered investigation state");

        timerCoroutine = null; //reset timer as appropriate
        CurrInvestigationTimer = 0.1f; 

        character.StartCoroutine(DisplayAndUpdateRing());
        //display ring around player here
        investigationCoroutine = StareAndFacePlayer(); //always begins here
        yield return character.StartCoroutine(investigationCoroutine);
    }

    public IEnumerator DisplayAndUpdateRing(){
        character.TargetPlayer.StealthRing.SetActive(true);
        ringUp = true;
        while(ringUp){
            //update direction
            // Vector3 dir = (character.transform.position - character.TargetPlayer.transform.position).normalized;
            character.TargetPlayer.StealthRing.transform.LookAt(character.transform.position, Vector3.up);
            yield return new WaitForEndOfFrame();
        }

        character.TargetPlayer.StealthRing.SetActive(false);
    }

    //stare at player, stationary while IN LOS
    private IEnumerator StareAndFacePlayer() {
        //todo: a stare animation?

        Array.Find(character.audioData, AudioData => AudioData.name == "spotted").Play(character.AudioSource);

        agent.SetDestination(character.transform.position); //halt movement
        isDecreasingDetection = false; //start by increasing detection
        
        if(timerCoroutine == null) { //start timer each first time only
            timerCoroutine = InvestigationTimer(); 
            character.StartCoroutine(timerCoroutine); 
        }


        //stare and face player here
        lookCoroutine = character.FacePlayer();
        character.StartCoroutine(lookCoroutine);


        animator.SetTrigger(Animator.StringToHash("Spotted"));
        //todo also make look at its own coroutine
        yield return new WaitUntil(() => !character.LOSOnPlayer() || CurrInvestigationTimer >= character.CurrSpotTimerThreshold); //keep incrementing until LOS is broken || caught
        

        //3 possibilities, 
        if(!character.LOSOnPlayer()) {
            character.StopCoroutine(lookCoroutine);
            //lost of sight before halfway
            if(character.CurrSpotTimerThreshold/2 > CurrInvestigationTimer) {
                //reverse timer, in place search
                investigationCoroutine = InPlaceSearch();
                character.StartCoroutine(investigationCoroutine);
                yield break;
            } else { //loss of sight after halfway, commit to ChainedDiscovery
                character.StartCoroutine(ChainedDiscovery());
                yield break;
            }
        } else { //caught, change global state, let mono behavior handle the rest
            character.PivotToAggro();
        }

    }

    private IEnumerator InPlaceSearch() {
        animator.SetTrigger(Animator.StringToHash("BrokeLOS"));
        agent.SetDestination(character.transform.position); //halt movement
        isDecreasingDetection = true; //start by DECREASING detection
        
        if(timerCoroutine == null) { //start timer each first time only
            timerCoroutine = InvestigationTimer();
            character.StartCoroutine(timerCoroutine); 
        }

        //in place search here

        yield return new WaitUntil(() => character.LOSOnPlayer() || CurrInvestigationTimer <= 0); //keep decrementing until LOS is regained || lost

        //two situations
        if(character.LOSOnPlayer()) { //if sight regained, back to staring
            investigationCoroutine = StareAndFacePlayer();//switch back to StareAndFace
            character.StartCoroutine(investigationCoroutine);
            yield break;
        } else { //otherwise, back to patrolling
            animator.SetTrigger(Animator.StringToHash("Normal"));
            character.SetStateDriver(new PatrolState(character, animator, agent));
            yield break;
        }

    }

    private IEnumerator InvestigationTimer() {
        float wfsIncrement = .1f;
        while(CurrInvestigationTimer < character.CurrSpotTimerThreshold && CurrInvestigationTimer >= 0) {
            character.stealthBar.fillAmount = CurrInvestigationTimer / character.CurrSpotTimerThreshold;
            yield return new WaitForSeconds(wfsIncrement); //space between each investigation timer tick - should be the same as think cycle
            CurrInvestigationTimer = isDecreasingDetection? CurrInvestigationTimer -= wfsIncrement : CurrInvestigationTimer += wfsIncrement;
            if(character.CurrSpotTimerThreshold/2 < CurrInvestigationTimer) {//start recording location after halfway point
                lastSeenPlayerLocation = character.TargetPlayer.transform.position;
            }
        }

    }

    public IEnumerator MoveToLocation(Vector3 location){ //Debug.Log("enteredMove");//shouldnt be a coroutine lol, no reason for the while statemtn
        agent.SetDestination(location);
        //character.debug(agent.stoppingDistance + " " + agent.remainingDistance);
        yield return new WaitUntil(() => (!agent.pathPending && agent.stoppingDistance > agent.remainingDistance));

        //if spotted again, 
    }

    public IEnumerator InvestigatePlayerPos(){ //Debug.Log("enteredMove");//shouldnt be a coroutine lol, no reason for the while statemtn
        animator.SetTrigger(Animator.StringToHash("InvestigateWalk"));
        agent.SetDestination(lastSeenPlayerLocation);
        //character.debug(agent.stoppingDistance + " " + agent.remainingDistance);
        yield return new WaitUntil(() => (!agent.pathPending && agent.stoppingDistance > agent.remainingDistance) || character.LOSOnPlayer());

        //if spotted again, 
    }

    private IEnumerator ChainedDiscovery() { //move to last seen location AND investigate
        //Debug.Log("chain discovering");

        character.StopCoroutine(timerCoroutine); //stop the timer
        timerCoroutine = null;

        animator.SetTrigger(Animator.StringToHash("BrokeLOS"));

        //"in place search" variation - as grace period
        float temp = 3f;
        while (temp > 0) {
            if(character.LOSOnPlayer()) break;
            temp -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if(character.LOSOnPlayer()) { //if sight regained, back to staring
            investigationCoroutine = StareAndFacePlayer();//switch back to StareAndFace
            character.StartCoroutine(investigationCoroutine);
            yield break;
        }

        //move to last seen location
        investigationCoroutine = InvestigatePlayerPos();
        yield return character.StartCoroutine(investigationCoroutine);

        if(character.LOSOnPlayer()) { //if sight regained, back to staring
            investigationCoroutine = StareAndFacePlayer();//switch back to StareAndFace
            character.StartCoroutine(investigationCoroutine);
            yield break;
        }

        animator.SetTrigger(Animator.StringToHash("SearchLastPos"));
        //go back to in place search
        investigationCoroutine = InPlaceSearch();
        character.StartCoroutine(investigationCoroutine);
        yield break;
    }

    public override IEnumerator OnStateExit() { 
        //stop displaying ring
        ringUp = false;

        //disable to exit possibilites (being to aggro and to patrol)
        if(lookCoroutine != null) character.StopCoroutine(lookCoroutine);
        if(timerCoroutine != null) character.StopCoroutine(timerCoroutine); 
        yield return null;
    }
}

public class DefaultAIAggroState : AIThinkState { 
    IEnumerator facePlayerRoutine;


    public DefaultAIAggroState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetLayerWeight(1, 1);

        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        yield break;
    }

   public override IEnumerator OnStateExit() {
        if(facePlayerRoutine != null) character.StopCoroutine(facePlayerRoutine);
        yield break;
    }
}

public class DefenseState : AIThinkState {
    IEnumerator defenseRoutine, facePlayerRoutine;
    //default block quota
    public DefenseState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}
    
    public override IEnumerator OnStateEnter() {
        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        defenseRoutine = Defense();
        yield return character.StartCoroutine(defenseRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    private IEnumerator Defense() {
        //face player, stand still todo
        //agent.SetDestination(character.transform.position);

        character.SetStateDriver(new BlockState(character, animator));
        yield return new WaitForSeconds(0.5f);
        character.SetStateDriver(new DefaultCombatState(character, animator));
        
        
    }

    public override IEnumerator OnStateExit() {
        if(facePlayerRoutine != null) character.StopCoroutine(facePlayerRoutine);
        if(defenseRoutine != null) character.StopCoroutine(defenseRoutine);
        yield break;
    }
}

public class ChaseState : AIThinkState {
    IEnumerator chaseRoutine, facePlayerRoutine;

    public ChaseState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}

    public override IEnumerator OnStateEnter() {
        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        chaseRoutine = character.ChasePlayer(character.tooFarFromPlayerDistance-3f);
        yield return character.StartCoroutine(chaseRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    public override IEnumerator OnStateExit() {
        //sotp guy from movin aboot
        if(facePlayerRoutine != null) character.StopCoroutine(facePlayerRoutine);
        if(chaseRoutine != null) character.StopCoroutine(chaseRoutine);
        agent.SetDestination(character.transform.position);
        yield break;
        
    }
}
public class BackAwayState : AIThinkState {
    IEnumerator backAwayRoutine, facePlayerRoutine;

    float originSpeed;

    public BackAwayState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) { }

    public override IEnumerator OnStateEnter() {
        // originSpeed = agent.speed;
        // agent.speed *= UnityEngine.Random.Range(0.3f, 1);

        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        backAwayRoutine = character.SpaceFromPlayer(character.backAwayDistance);
        yield return character.StartCoroutine(backAwayRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    public override IEnumerator OnStateExit() {
        // agent.speed = originSpeed;
        if(backAwayRoutine != null) character.StopCoroutine(backAwayRoutine);
        if(facePlayerRoutine != null) character.StopCoroutine(facePlayerRoutine);
        yield break;

    }

}
public class ShoveState : AIThinkState {
    IEnumerator shoveRoutine;

    MeleeMove shove;

    public ShoveState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {
        try { shove = character.MeleeAttacks["shove"]; } catch { Debug.LogWarning("no shove attack in melee moves"); }
    }

    public override IEnumerator OnStateEnter() {
        character.transform.LookAt(character.transform.position);
        character.SetStateDriver(new AttackState(character, animator, shove));
        yield return new WaitWhile(() => character.genericState is AttackState);
        shoveRoutine = character.SpaceFromPlayer(5f);
        yield return character.StartCoroutine(shoveRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }


}

//Circling State
public class CirclingState : AIThinkState {
    //basically worked off of spacing state

    IEnumerator circlingRoutine, facePlayerRoutine;

    float originSpeed;

    public CirclingState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) { }

    public override IEnumerator OnStateEnter() {
        //So, the overarching idea is that there exists a circle around the player that the enemies would like to occupy a position on the radius
        //As such, we will attempting to find such a space on the circle of the player, and move towards it. Of course, there are very many
        //technical issues, esp when we have to consider the relative positioning of the other enemies on the circle, the movement of the player, etc.
        //So, basically: When circling, we continue to find points on the circle, and we move towrads it. 
        //So if the player is constantly moving, then the circling AIs are constantly attempting to reach a point on the circle, which should be fine enough for the
        //'circling' aspect. 
        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        circlingRoutine = CirclingAIHelper();
        yield return character.StartCoroutine(circlingRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    private IEnumerator CirclingAIHelper(){
        NavMeshHit hit;
        Vector3 targetmove = CirclingPositionHelper();
        if(NavMesh.SamplePosition(targetmove, out hit, character.backAwayDistance + 5f, NavMesh.AllAreas)
            || agent.FindClosestEdge(out hit)) {
            agent.SetDestination(hit.position);
            }

        yield return new WaitWhile(() => agent.stoppingDistance < agent.remainingDistance || agent.pathPending);
    }


    private Vector3 CirclingPositionHelper() {
        //Basically where all the acutal math happense, as much as Math.random or whatever the fuck can be called math
        //inital thoughts, during the meeting, we had said that we intended towards writing code that will
        //basically hive mind the enemies into evenly distributi9ng themselves around the player circle
        //However, given that we cannot access the number of cirlcing AIs, (this might be false) and then also having the evenly distrubute themselves
        //has made the first difficult, thus resulting in an abortion of said idea

        //instead, we propose the following:
        //Approach A: Just choose a random position on the circle/annulus around the player
        //Pros: will be pretty fucking random, which more or less suits the 'circling' state of the AI
        //Cons: Can very possibly cause enemy density

        //I have thought of another approach, but am uncertain if it is possible, as such, will be implementing the one above
        float minRadius = 10;
        float maxRadius = 12;
        float circleTolerance = 2;

        //Basically loop until we get a position within tolerance, max from player to AI + tolerance
        var randomDirection = (UnityEngine.Random.insideUnitCircle * character.TargetPlayer.transform.position).normalized;
        var randomDistance = UnityEngine.Random.Range(minRadius, maxRadius);
        Vector3 changed = randomDirection * randomDistance;
        Vector3 targetLocation = (character.TargetPlayer.transform.position + changed);

        Vector3 compareLocation = character.TargetPlayer.transform.position + character.transform.position;

        int loopcount = 0;
        //either exceed max loop count or less than distance
        while ((loopcount <= 13) && ((character.transform.position + targetLocation).magnitude - circleTolerance > compareLocation.magnitude)) {
            randomDirection = (UnityEngine.Random.insideUnitCircle * character.TargetPlayer.transform.position).normalized;
            randomDistance = UnityEngine.Random.Range(minRadius, maxRadius);
            changed = randomDirection * randomDistance;
            targetLocation = (character.TargetPlayer.transform.position + changed);
            loopcount += 1;
        }

        // Debug.Log("Currently Circling Towards Pos: " + targetLocation); 
        return targetLocation;
    }    

    public override IEnumerator OnStateExit() {
        agent.SetDestination(character.transform.position);
        if(facePlayerRoutine != null) character.StopCoroutine(facePlayerRoutine);
        if(circlingRoutine != null) character.StopCoroutine(circlingRoutine);
        yield break;
    }
}

//approach and shank
//todo: make charge a unique state
public class OffenseState : AIThinkState {

    protected MeleeMove chosenAttack;
    IEnumerator offenseRoutine;
    IEnumerator subRoutine;
    IEnumerator facePlayerRoutine;
    public OffenseState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {
        try { this.chosenAttack = character.MeleeAttacks["default"]; } catch { Debug.LogWarning("some cunt don't have default attack"); }
    }

    public OffenseState(AIHandler character, Animator animator, NavMeshAgent agent, MeleeMove chosenAttack) : base(character, animator, agent) {
        this.chosenAttack = chosenAttack;
    }


    public override IEnumerator OnStateEnter() {
        // Debug.Log(AIHandler.CombatAI.Count);
        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        offenseRoutine = Offense(chosenAttack);
        yield return character.StartCoroutine(offenseRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    private IEnumerator Offense(MeleeMove chosenAttack) {


        subRoutine = character.ChasePlayer(chosenAttack.range);
        yield return character.StartCoroutine(subRoutine);

        Debug.Log("AI attempting an attack...");
        character.SetStateDriver(new AttackState(character, animator, chosenAttack));
        
        //once attack is finished, cooldown a touch maybe
        float timer = chosenAttack.endlag + chosenAttack.startup;
        while(timer >= 0) {
            if(character.genericState is StaggerState) { 
                Debug.Log("offense interupted for stagger"); 
                yield break; 
            }
            
            yield return new WaitForSeconds(.1f);
            timer -= .1f;
        }
        

    }

    public override IEnumerator OnStateExit() {
        if(facePlayerRoutine != null) character.StopCoroutine(facePlayerRoutine);
        if(subRoutine != null) character.StopCoroutine(subRoutine);
        if(offenseRoutine != null) character.StopCoroutine(offenseRoutine);
        agent.SetDestination(character.transform.position);
        yield break;
    }

}

//special offense - charge
public class ChargeState : OffenseState {
    float chargeSpeedMult;
    public ChargeState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {
        chargeSpeedMult = 3f;
        try { this.chosenAttack = character.MeleeAttacks["chargeMove"]; } catch { Debug.LogWarning("some cunt don't have charge attack"); }

    }

    public override IEnumerator OnStateEnter(){
        // Debug.Log("charge incoming");
        //brief moment of pause- indicating to the player "he comin"
        yield return new WaitForSeconds(1f); 

        animator.SetTrigger(Animator.StringToHash("charge"));
        agent.speed *= chargeSpeedMult;
        yield return base.OnStateEnter();
    }

    public override IEnumerator OnStateExit(){
        agent.speed /= chargeSpeedMult;
        yield return base.OnStateExit();

    }

}

// public class SpacingState : AIThinkState {
//     IEnumerator facePlayerRoutine;
//     IEnumerator spacingRoutine;

//     public SpacingState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}

//     public override IEnumerator OnStateEnter() {
//         facePlayerRoutine = character.FacePlayer();
//         character.StartCoroutine(facePlayerRoutine);
//         spacingRoutine = SpaceFromAI();
//         yield return character.StartCoroutine(spacingRoutine);
//         character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
//     }

//     private IEnumerator SpaceFromAI(){
//         NavMeshHit hit;
//         Vector3 movePoint = FindMostViablePosition();
//         if(NavMesh.SamplePosition(movePoint, out hit, character.backAwayDistance + 5f, NavMesh.AllAreas)
//             || agent.FindClosestEdge(out hit)) {
//             agent.SetDestination(hit.position);
//         }

//         yield return new WaitWhile(() => agent.stoppingDistance < agent.remainingDistance || agent.pathPending);
//     }

//     private Vector3 FindMostViablePosition() {
//         //cast a circle around the player AND spacing ai of radiusbackAwayDistance
//         float circDistance = Vector3.Distance(character.transform.position, character.TargetPlayer.transform.position);
//         float selfToMid = (circDistance * circDistance) / (2 * circDistance);
//         float midToEdge = (5 + character.backAwayDistance) * (5 + character.backAwayDistance) - (selfToMid * selfToMid);

//         //find all RADIUS intersections between those circles
//         Vector3 intersectionPoint = character.transform.position + selfToMid * (character.TargetPlayer.transform.position - character.transform.position) / circDistance;
//         Vector3 pointCheck1 = new Vector3(intersectionPoint.x + midToEdge * (character.TargetPlayer.transform.position.z - character.transform.position.z) / circDistance, character.currProximateAIPosition.y, intersectionPoint.z - midToEdge * (character.TargetPlayer.transform.position.x - character.transform.position.x) / circDistance);
//         Vector3 pointCheck2 = new Vector3(intersectionPoint.x - midToEdge * (character.TargetPlayer.transform.position.z - character.transform.position.z) / circDistance, character.currProximateAIPosition.y, intersectionPoint.z + midToEdge * (character.TargetPlayer.transform.position.x - character.transform.position.x) / circDistance);

//         //pick the intersection FURTHEST from the AI being spaced from
//         return (pointCheck1 - character.currProximateAIPosition).sqrMagnitude > (pointCheck2 - character.currProximateAIPosition).sqrMagnitude ? pointCheck1 : pointCheck2;
//         //store this point as closestPoint;

//         //cast another circle around the player  AND spacing of chase distance (within ucnertainty)
//         //find all RADIUS intersections between those circles
//         //pick the intersection FURTHEST from the AI being spaced from
//         //store this point as furthestPoint;

//         //create a line between closestPoint and furthestPoint, 
//         //then return a random point on it

//     }    

//     public override IEnumerator OnStateExit() {
//         agent.SetDestination(character.transform.position);
//         if(facePlayerRoutine != null) character.StopCoroutine(facePlayerRoutine);
//         if(spacingRoutine != null) character.StopCoroutine(spacingRoutine);
//         yield break;
//     }

// }
