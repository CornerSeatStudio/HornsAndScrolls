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
    public MeleeMove chosenMove {get; private set;}

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
        yield return character.StartCoroutine(attackHandler.FindTarget(chosenMove));
        //if no targets in range
        if (attackHandler.chosenTarget == null) {
            Debug.Log("no targets in range");
            yield return new WaitForSeconds(chosenMove.startup + chosenMove.endlag); //swing anyways
            character.SetStateDriver(new DefaultState(character, animator, attackHandler));
        }

        //upon completion of finding target/at attack move setup, START listening
        yield return new WaitForSeconds(chosenMove.startup); //assumption: start up == counter window


        attackHandler.chosenTarget.AttackResponse(chosenMove.damage, character);
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
    protected MeleeMove block;

    public BlockState(CharacterHandler character, Animator animator, MeleeRaycastHandler attackHandler) : base(character, animator, attackHandler) {
        block = character.MeleeMoves["block"]; //TEMPORARY, 
    }

    public override IEnumerator OnStateEnter() { 
        currBlockRoutine = CheckBlockRange();
        yield return character.StartCoroutine(currBlockRoutine);
    } 

    //provides information on blocking CONTINUOUSLY
    private IEnumerator CheckBlockRange() {
        while (true) {
            yield return character.StartCoroutine(attackHandler.FindTarget(block)); //run find target while blocking
            //if the attackHandler.chosenTarget exists, the block IS ALLOWED
        }
    }

    public override IEnumerator OnStateExit() { 
        if(currBlockRoutine != null) character.StopCoroutine(currBlockRoutine); //stop block coroutine
        attackHandler.chosenTarget = null; //empty attackHandler.chosenTarget
        yield break;
    } 
}

public class CounterState : CombatState {
    protected IEnumerator currCounterRoutine;
    protected MeleeMove block;

    public CounterState(CharacterHandler character, Animator animator, MeleeRaycastHandler attackHandler) : base(character, animator, attackHandler) {}

    public override IEnumerator OnStateEnter() {        
        //trigger counter event
        //if enemy is attacking you specifically (dont trigger if coming from a specific state)
        //shouldnt be done here, should be done in attack response via comparing type
        currCounterRoutine = CheckCounterRange();
        yield return character.StartCoroutine(currCounterRoutine);
        if(attackHandler.chosenTarget != null && attackHandler.chosenTarget.combatState is AttackState) {
        }

        yield return new WaitForSeconds(0.3f); //where param is counter timeframe
        character.SetStateDriver(new BlockState(character, animator, attackHandler));
    }

    //check ONCE for counter check
    private IEnumerator CheckCounterRange() {
        yield return character.StartCoroutine(attackHandler.FindTarget(block));
    }

    public override IEnumerator OnStateExit() {
        Debug.Log("exiting counter state");
        if (currCounterRoutine != null) character.StopCoroutine(currCounterRoutine);
        yield break;
    }

}

public class DodgeState : CombatState {
    public DodgeState(CharacterHandler character, Animator animator, MeleeRaycastHandler attackHandler) : base(character, animator, attackHandler) {}

}

public class StaggerState : CombatState {
    public StaggerState(CharacterHandler character, Animator animator, MeleeRaycastHandler attackHandler) : base(character, animator, attackHandler) {}

}
