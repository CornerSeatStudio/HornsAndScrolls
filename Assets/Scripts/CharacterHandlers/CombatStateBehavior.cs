using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DefaultState : CombatState {
    public DefaultState(CharacterHandler character, Animator animator, MeleeRaycastHandler attackHandler) : base(character, animator, attackHandler) {}

    public override IEnumerator OnStateEnter() {
        Debug.Log("entering default combat state");
        yield break;
    }

    public override IEnumerator OnStateExit() {
        Debug.Log("exiting default combat state");
        yield break;
    }
    //probably just animator stuff

}

public class AttackState : CombatState {
    protected IEnumerator currAttackCoroutine;
    protected MeleeMove chosenMove;

    public AttackState(CharacterHandler character, Animator animator, MeleeRaycastHandler attackHandler) : base(character, animator, attackHandler) {
        chosenMove = character.MeleeMoves["default"]; //TEMPORARY, WILL MAKE POLYMORPHIC
        //Debug.Log(chosenAttackMove.name + ": " + chosenAttackMove.startup);
    }

    public override IEnumerator OnStateEnter() { 
        Debug.Log("entering attacking state");
        currAttackCoroutine = FindTargetAndDealDamage();
        yield return character.StartCoroutine(currAttackCoroutine);
        character.SetStateDriver(new DefaultState(character, animator, attackHandler));
    }    

    protected virtual IEnumerator FindTargetAndDealDamage(){
        yield return attackHandler.FindTarget(chosenMove);
        //if no targets in range
        if (attackHandler.chosenTarget == null) {
            Debug.Log("no targets in range");
            yield return new WaitForSeconds(chosenMove.startup + chosenMove.endlag); //swing anyways
            character.SetStateDriver(new DefaultState(character, animator, attackHandler));
        }

        //upon completion of finding target/at attack move setup, START listening
        yield return new WaitForSeconds(chosenMove.startup); //assumption: start up == counter window

        attackHandler.chosenTarget.TakeDamage(chosenMove.damage);
        //replcae with AttackResponse ->

        //during the endlag phase, check again
        //if I was hit && I am using blockable attack, stagger instead
        yield return new WaitForSeconds(chosenMove.endlag);
        character.SetStateDriver(new DefaultState(character, animator, attackHandler));
    }

 
    public override IEnumerator OnStateExit() {
        Debug.Log("exiting attacking state");
        if(currAttackCoroutine != null) character.StopCoroutine(currAttackCoroutine); 
        yield break;
    }

}


public class BlockState : CombatState {
    protected IEnumerator currBlockRoutine;
    protected MeleeMove chosenMove;

    public BlockState(CharacterHandler character, Animator animator, MeleeRaycastHandler attackHandler) : base(character, animator, attackHandler) {
        chosenMove = character.MeleeMoves["block"]; //TEMPORARY, 
    }

    public override IEnumerator OnStateEnter() { 
        currBlockRoutine = CheckBlockRange();
        yield return character.StartCoroutine(currBlockRoutine);
    } 
    //provides information on blocking
    private IEnumerator CheckBlockRange() {
        while (true) {
            yield return attackHandler.FindTarget(chosenMove); //run find target while blocking
            //if the attackHandler.chosenTarget exists, the block IS ALLOWED
        }
    }

    public override IEnumerator OnStateExit() { 
        if(currBlockRoutine != null) character.StopCoroutine(currBlockRoutine); //stop block coroutine
        attackHandler.chosenTarget = null; //empty attackHandler.chosenTarget
        yield break;
    } 
}


