using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DefaultCombatState : CombatState {
    public DefaultCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).combatMoveSpeed;
        yield break;
    }

    // public override IEnumerator OnStateExit() {
    //  //   animator.SetBool(character.AnimationHashes["IsAggro"], false);
    //     animator.SetBool(character.AnimationHashes["WeaponOut"], false);
    //     //Debug.Log("exiting default combat state");
    //     yield break;
    // }

}

public class AttackState : CombatState {
    protected IEnumerator currAttackCoroutine;
    public MeleeMove chosenMove {get; private set;}

    public AttackState(CharacterHandler character, Animator animator) : base(character, animator) {
        chosenMove = character.MeleeAttacks["default"]; 
    }

    public AttackState(CharacterHandler character, Animator animator, MeleeMove chosenMove) : base(character, animator) {
        this.chosenMove = chosenMove;
    }
    public override IEnumerator OnStateEnter() { 
        //Debug.Log("entering attacking state");
        currAttackCoroutine = FindTargetAndDealDamage();
        yield return character.StartCoroutine(currAttackCoroutine);
        character.SetStateDriver(new DefaultCombatState(character, animator));
        animator.SetBool(character.AnimationHashes["IsAttacking"], true);
    }    

    protected virtual IEnumerator FindTargetAndDealDamage(){
        CharacterHandler chosenTarget = character.FindTarget(chosenMove);
        
        //Debug.Log(chosenMove.angle + " " + chosenMove.range);
        //if no targets in range
        if (chosenTarget == null) {
            yield return new WaitForSeconds(chosenMove.startup + chosenMove.endlag); //swing anyways
            character.SetStateDriver(new DefaultCombatState(character, animator));
            yield break;
        }
        //upon completion of finding target/at attack move setup, START listening
        yield return new WaitForSeconds(chosenMove.startup); //assumption: start up == counter window


        chosenTarget.AttackResponse(chosenMove.damage, character);
        //during the endlag phase, check again
        //if I was hit && I am using blockable attack, stagger instead
        yield return new WaitForSeconds(chosenMove.endlag);
        character.SetStateDriver(new DefaultCombatState(character, animator));
    }

 
    public override IEnumerator OnStateExit() {
        //Debug.Log("exiting attacking state");
        if(currAttackCoroutine != null) character.StopCoroutine(currAttackCoroutine); 
        animator.SetBool(character.AnimationHashes["IsAttacking"], false);
        yield break;
    }
}

public class BlockState : CombatState {
    protected IEnumerator currBlockRoutine;
    protected MeleeMove block;

    public BlockState(CharacterHandler character, Animator animator) : base(character, animator) {
        block = character.MeleeBlock; 
    }

    public override IEnumerator OnStateEnter() { 
        currBlockRoutine = CheckBlockRange();
        animator.SetBool(character.AnimationHashes["IsBlocking"], true);
        character.StartCoroutine(currBlockRoutine);
        yield break;
    } 

    //provides information on blocking CONTINUOUSLY
    private IEnumerator CheckBlockRange() {
        CharacterHandler possibleTarget;
        while (true) {
            possibleTarget = character.FindTarget(block);
            if(possibleTarget != null) Debug.Log("block would be valid");
            //if the attackHandler.chosenTarget exists, the block IS ALLOWED
        }
    }

    public override IEnumerator OnStateExit() { 
        if(currBlockRoutine != null) character.StopCoroutine(currBlockRoutine); //stop block coroutine
        animator.SetBool(character.AnimationHashes["IsBlocking"], false);
        yield break;
    } 

}

public class CounterState : CombatState {
    protected IEnumerator currCounterRoutine;
    protected MeleeMove block;

    public CounterState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {   
        Debug.Log("entering counter");     
        //trigger counter event
        //if enemy is attacking you specifically (dont trigger if coming from a specific state)
        //shouldnt be done here, should be done in attack response via comparing type
        
        animator.SetBool(character.AnimationHashes["IsBlocking"], true);

        currCounterRoutine = CheckCounterRange();
        character.StartCoroutine(currCounterRoutine);


        yield return new WaitForSeconds(0.3f); //where param is counter timeframe
        character.SetStateDriver(new BlockState(character, animator));
    }

    //check ONCE for counter check todo
    private IEnumerator CheckCounterRange() {
        CharacterHandler possibleTarget;
        while (true) {
            possibleTarget = character.FindTarget(block);
            if(possibleTarget != null) Debug.Log("counter would be valid");
            //if the attackHandler.chosenTarget exists, the block IS ALLOWED
        }
    }

    public override IEnumerator OnStateExit() {
        Debug.Log("exiting counter state");
        if (currCounterRoutine != null) character.StopCoroutine(currCounterRoutine);
        yield break;
    }

}

public class DodgeState : CombatState {
    Vector3 direction;

    public DodgeState(CharacterHandler character, Animator animator, Vector3 direction) : base(character, animator) {
        this.direction = direction;
    }


    public override IEnumerator OnStateEnter() {
        animator.ResetTrigger(character.AnimationHashes["Dodging"]);
        animator.SetTrigger(character.AnimationHashes["Dodging"]);
        yield return new WaitForSeconds((character as PlayerHandler).dodgeTime);
        character.SetStateDriver(new DefaultCombatState(character, animator));
    }

    // public override IEnumerator OnStateExit() {
    //     yield break;
    // }

}

public class StaggerState : CombatState {
    public StaggerState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {  
        yield return new WaitForSeconds(.5f); //stagger time
        character.SetStateDriver(new DefaultCombatState(character, animator));      
    }


}

public class DeathState : CombatState {
    public DeathState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter(){
        //change globalState
        (character as AIHandler).GlobalState = GlobalState.DEAD;
        //stop APPROPRITE coroutines
        (character as AIHandler).Detection.IsAlive = false; //detection
        //ragdoll 
        (character as AIHandler).GetComponent<Collider>().enabled = false;
        Rigidbody[] rigidbodies = character.GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in rigidbodies){
            if(rb.gameObject != character.gameObject){
                rb.isKinematic = false;
            }
        }

        Collider[] colliders = character.GetComponentsInChildren<Collider>();
        foreach(Collider col in colliders) {
            if (col.gameObject != character.gameObject) {
                col.enabled = true;
            } 
        }

        //disable animator as last step
        animator.enabled = false;
        Debug.Log("agh i haveth been a slain o");
        yield return null;
    }

}