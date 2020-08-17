﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : AIThinkState {
    private IEnumerator idleCoroutine, LOSCoroutine;

    public IdleState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}
    
    public override IEnumerator OnStateEnter() { //Debug.Log("enteredIdle, timer: " + character.idleTimeAtWaypoint);
        idleCoroutine = IdleTimer(character.idleTimeAtWaypoint);
        yield return character.StartCoroutine(idleCoroutine);
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
    private bool isDecreasingDetection = false;

    public InvestigationState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}

    public override IEnumerator OnStateEnter() {
        //Debug.Log("entered investigation state");

        timerCoroutine = null; //reset timer as appropriate
        CurrInvestigationTimer = 0.1f; 

        investigationCoroutine = StareAndFacePlayer(); //always begins here
        yield return character.StartCoroutine(investigationCoroutine);
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
            animator.SetBool(Animator.StringToHash("Combat"), true); //todo: to be put in separate class
            // character.layerWeightRoutine = character.LayerWeightDriver(1, 0, 1, .3f);
            // character.StartCoroutine(character.layerWeightRoutine);
            character.GlobalState = GlobalState.AGGRO;
            character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
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
    IEnumerator defenseRoutine;
    //default block quota
    public DefenseState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}
    
    public override IEnumerator OnStateEnter() {
        defenseRoutine = Defense();
        yield return character.StartCoroutine(defenseRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    private IEnumerator Defense() {
        //face player, stand still todo
        character.transform.LookAt(character.TargetPlayer.transform); //ok to not be in coroutine cause its fast
        //agent.SetDestination(character.transform.position);

        character.SetStateDriver(new BlockState(character, animator));
        yield return new WaitForSeconds(1.5f);
        character.SetStateDriver(new DefaultCombatState(character, animator));
        
    }

    public override IEnumerator OnStateExit() {
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

    public BackAwayState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) { }

    public override IEnumerator OnStateEnter() {
        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        backAwayRoutine = character.SpaceFromPlayer(character.backAwayDistance);
        yield return character.StartCoroutine(backAwayRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    public override IEnumerator OnStateExit() {
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

//approach and shank
public class OffenseState : AIThinkState {

    MeleeMove chosenAttack;
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
        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        offenseRoutine = Offense(chosenAttack);
        yield return character.StartCoroutine(offenseRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    private IEnumerator Offense(MeleeMove chosenAttack) {
        subRoutine = character.ChasePlayer(chosenAttack.range - 3f);
        yield return character.StartCoroutine(subRoutine);

        //Debug.Log("AI attempting an attack...");
        character.SetStateDriver(new AttackState(character, animator, chosenAttack));
        
        //once attack is finished, cooldown a touch maybe
        float timer = chosenAttack.endlag + chosenAttack.startup;
        while(timer >= 0) {
            if(character.genericState is StaggerState) { 
                Debug.Log("offense interupt"); 
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

public class SpacingState : AIThinkState {
    IEnumerator facePlayerRoutine;
    IEnumerator spacingRoutine;

    public SpacingState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}

    public override IEnumerator OnStateEnter() {
        facePlayerRoutine = character.FacePlayer();
        character.StartCoroutine(facePlayerRoutine);
        spacingRoutine = SpaceFromAI();
        yield return character.StartCoroutine(spacingRoutine);
        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    private IEnumerator SpaceFromAI(){
        NavMeshHit hit;
        Vector3 movePoint = FindMostViablePosition();
        if(NavMesh.SamplePosition(movePoint, out hit, character.backAwayDistance + 5f, NavMesh.AllAreas)
            || NavMesh.FindClosestEdge(movePoint, out hit, NavMesh.AllAreas)) {
            agent.SetDestination(hit.position);
        }

        yield return new WaitWhile(() => agent.stoppingDistance < agent.remainingDistance || agent.pathPending);

        character.SetStateDriver(new DefaultAIAggroState(character, animator, agent));
    }

    private Vector3 FindMostViablePosition() {
        //cast a circle around the player AND spacing ai of radiusbackAwayDistance
        float circDistance = Vector3.Distance(character.transform.position, character.TargetPlayer.transform.position);
        float selfToMid = (circDistance * circDistance) / (2 * circDistance);
        float midToEdge = (5 + character.backAwayDistance) * (5 + character.backAwayDistance) - (selfToMid * selfToMid);

        //find all RADIUS intersections between those circles
        Vector3 intersectionPoint = character.transform.position + selfToMid * (character.TargetPlayer.transform.position - character.transform.position) / circDistance;
        Vector3 pointCheck1 = new Vector3(intersectionPoint.x + midToEdge * (character.TargetPlayer.transform.position.z - character.transform.position.z) / circDistance, character.currProximateAIPosition.y, intersectionPoint.z - midToEdge * (character.TargetPlayer.transform.position.x - character.transform.position.x) / circDistance);
        Vector3 pointCheck2 = new Vector3(intersectionPoint.x - midToEdge * (character.TargetPlayer.transform.position.z - character.transform.position.z) / circDistance, character.currProximateAIPosition.y, intersectionPoint.z + midToEdge * (character.TargetPlayer.transform.position.x - character.transform.position.x) / circDistance);

        //pick the intersection FURTHEST from the AI being spaced from
        return (pointCheck1 - character.currProximateAIPosition).sqrMagnitude > (pointCheck2 - character.currProximateAIPosition).sqrMagnitude ? pointCheck1 : pointCheck2;
        //store this point as closestPoint;

        //cast another circle around the player  AND spacing of chase distance (within ucnertainty)
        //find all RADIUS intersections between those circles
        //pick the intersection FURTHEST from the AI being spaced from
        //store this point as furthestPoint;

        //create a line between closestPoint and furthestPoint, 
        //then return a random point on it

    }    

    public override IEnumerator OnStateExit() {
        agent.SetDestination(character.transform.position);
        if(facePlayerRoutine != null) character.StopCoroutine(facePlayerRoutine);
        if(spacingRoutine != null) character.StopCoroutine(spacingRoutine);
        yield break;
    }

}