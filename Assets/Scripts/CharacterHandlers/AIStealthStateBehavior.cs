using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : AIStealthState {
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

public class PatrolState : AIStealthState {
    private IEnumerator moveToLocationCoroutine, LOSCoroutine;

    public PatrolState(AIHandler character, Animator animator, NavMeshAgent agent) : base(character, animator, agent) {}

    public override IEnumerator OnStateEnter() { //Debug.Log("enteredPatrol");
        if(character.NextWaypointLocation == null) { Debug.LogWarning("no waypoints on some cunt"); character.SetStateDriver(new IdleState(character, animator, agent)); };
        
        animator.SetBool(character.AnimationHashes["IsPatrol"], true);
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
        animator.SetBool(character.AnimationHashes["IsPatrol"], false);
        if(moveToLocationCoroutine != null) character.StopCoroutine(moveToLocationCoroutine); //stop coroutine
        yield return null;
    }

}

public class InvestigationState : AIStealthState {
    public float CurrInvestigationTimer {get; private set;}
    private Vector3 lastSeenPlayerLocation;
    private IEnumerator timerCoroutine, investigationCoroutine;
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

        agent.isStopped = true; //halt movement
        isDecreasingDetection = false; //start by increasing detection
        
        if(timerCoroutine == null) { //start timer each first time only
            timerCoroutine = InvestigationTimer(); 
            character.StartCoroutine(timerCoroutine); 
        }


        //stare and face player here
        //todo animator.SetBool("IsStaring", true);

        yield return new WaitUntil(() => !character.LOSOnPlayer() || CurrInvestigationTimer >= character.spotTimerThreshold); //keep incrementing until LOS is broken || caught

        //if condition break, stop staring animation (todo)
        //todo animator.SetBool("IsStaring", false);

        //3 possibilities, 
        if(!character.LOSOnPlayer()) {
            //lost of sight before halfway
            if(character.spotTimerThreshold/2 > CurrInvestigationTimer) {
                //reverse timer, but switch to ChainedDiscovery
                isDecreasingDetection = true;
                investigationCoroutine = InPlaceSearch();
                character.StartCoroutine(investigationCoroutine);
                yield break;
            } else { //loss of sight after halfway, commit to ChainedDiscovery
                character.StartCoroutine(ChainedDiscovery());
                yield break;
            }
        } else { //caught, change global state, let mono behavior handle the rest
            animator.SetBool(character.AnimationHashes["IsAggroWalk"], true); //todo: to be put in separate class
            character.GlobalState = GlobalState.AGGRO;
            character.ThrowStateToGCDriver();
        }

    }

    private IEnumerator InPlaceSearch() {
        animator.SetBool(character.AnimationHashes["IsSearching"], true);
        agent.isStopped = true; //halt movement
        isDecreasingDetection = true; //start by DECREASING detection
        
        if(timerCoroutine == null) { //start timer each first time only
            timerCoroutine = InvestigationTimer();
            character.StartCoroutine(timerCoroutine); 
        }

        //in place search here

        yield return new WaitUntil(() => character.LOSOnPlayer() || CurrInvestigationTimer <= 0); //keep decrementing until LOS is regained || lost

        animator.SetBool(character.AnimationHashes["IsSearching"], false);

        //two situations
        if(character.LOSOnPlayer()) { //if sight regained, back to staring
            isDecreasingDetection = false; //reverse timer
            investigationCoroutine = StareAndFacePlayer();//switch back to StareAndFace
            character.StartCoroutine(investigationCoroutine);
            yield break;
        } else { //otherwise, back to patrolling
            agent.isStopped = false; //resume movement
            character.SetStateDriver(new PatrolState(character, animator, agent));
            yield break;
        }

    }

    public IEnumerator MoveToLocation(Vector3 location){ //Debug.Log("enteredMove");//shouldnt be a coroutine lol, no reason for the while statemtn
        
        animator.SetBool(character.AnimationHashes["IsAggroWalk"], true);
        
        agent.SetDestination(location);
        //character.debug(agent.stoppingDistance + " " + agent.remainingDistance);
        yield return new WaitUntil(() => !agent.pathPending && agent.stoppingDistance > agent.remainingDistance);
        //Debug.Log(agent.pathPending + " " + (agent.stoppingDistance < agent.remainingDistance));
        animator.SetBool(character.AnimationHashes["IsAggroWalk"], false);
    }

    private IEnumerator InvestigationTimer() {
        float wfsIncrement = .1f;
        while(CurrInvestigationTimer < character.spotTimerThreshold && CurrInvestigationTimer >= 0) {
            yield return new WaitForSeconds(wfsIncrement); //space between each investigation timer tick - should be the same as think cycle
            CurrInvestigationTimer = isDecreasingDetection? CurrInvestigationTimer -= wfsIncrement : CurrInvestigationTimer += wfsIncrement;
            if(character.spotTimerThreshold/2 < CurrInvestigationTimer) {//start recording location after halfway point
                lastSeenPlayerLocation = character.targetPlayer.transform.position;
            }
        }

    }

    private IEnumerator ChainedDiscovery() { //move to last seen location AND investigate
        //Debug.Log("chain discovering");
        character.StopCoroutine(timerCoroutine); //stop the timer
        timerCoroutine = null;
        animator.SetBool(character.AnimationHashes["IsSearching"], true);

        //"in place search" variation - as grace period
        yield return new WaitForSeconds(3f);

        animator.SetBool(character.AnimationHashes["IsSearching"], false);

        agent.isStopped = false; //allow character to move after grace period
        //move to last seen location
        investigationCoroutine = MoveToLocation(lastSeenPlayerLocation);
        yield return character.StartCoroutine(investigationCoroutine);

        //go back to in place search
        investigationCoroutine = InPlaceSearch();
        character.StartCoroutine(investigationCoroutine);
        yield break;
    }

    public override IEnumerator OnStateExit() { Debug.Log("exiting investigation");
        agent.isStopped = false; //ensure movement can continue
        //disable to exit possibilites (being to aggro and to patrol)
        //todo animator.SetBool("isStaring", false);
        animator.SetBool(character.AnimationHashes["IsSearching"], false);
        if(timerCoroutine != null) character.StopCoroutine(timerCoroutine); //stop coroutine
        yield return null;
    }
}
