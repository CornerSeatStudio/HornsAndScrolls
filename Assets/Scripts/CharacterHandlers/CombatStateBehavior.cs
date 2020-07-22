using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;


public class UnsheathingCombatState : CombatState {
    IEnumerator sheathRoutine;

    public UnsheathingCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
      //  try { (character as PlayerHandler).parentToHand(); } catch { Debug.LogWarning("ai shoudnt be in this state"); }

        sheathRoutine = Sheath();
        yield return character.StartCoroutine(sheathRoutine);
        character.SetStateDriver(new DefaultCombatState(character, animator));
    }

    private IEnumerator Sheath(){
        yield return new WaitUntil(() => character.layerWeightRoutine == null);
        character.layerWeightRoutine = character.LayerWeightDriver(1, 0, 1, .3f);
        yield return character.StartCoroutine(character.layerWeightRoutine);

        animator.ResetTrigger(Animator.StringToHash("WeaponDraw"));
        animator.SetTrigger(Animator.StringToHash("WeaponDraw"));
        animator.SetBool(Animator.StringToHash("WeaponOut"), true); //if transitioning between sheathing and unsheathing, this overrides it too
        
        Array.Find(character.audioData, AudioData => AudioData.name == "unsheath").Play(character.AudioSource);

        yield return new WaitForSeconds(1.5f); //sheath time idk why its varied
        //yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(1).length);

    }

    public override IEnumerator OnStateExit() {
        if(sheathRoutine != null) character.StopCoroutine(sheathRoutine);
        yield break;
    }

}

public class SheathingCombatState : CombatState {
    IEnumerator sheathRoutine;

    public SheathingCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
 
        sheathRoutine = Sheath();
        yield return character.StartCoroutine(sheathRoutine);
        character.SetStateDriver(new IdleMoveState(character, animator));

    }

    private IEnumerator Sheath(){ animator.SetBool(Animator.StringToHash("WeaponOut"), false); 
        animator.ResetTrigger(Animator.StringToHash("WeaponDraw"));
        animator.SetTrigger(Animator.StringToHash("WeaponDraw"));
        
        Array.Find(character.audioData, AudioData => AudioData.name == "sheath").Play(character.AudioSource);

        yield return new WaitForSeconds(1.5f); //sheath time idk why its varied
       // yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(1).length);
    }

    public override IEnumerator OnStateExit() { //once drawn OR INTERUPTED
      //  try { (character as PlayerHandler).parentToSheath(); } catch { Debug.LogWarning("ai shoudnt be in this state"); }
        if(sheathRoutine != null) character.StopCoroutine(sheathRoutine);
        
        yield return new WaitUntil(() => character.layerWeightRoutine == null);
        character.layerWeightRoutine = character.LayerWeightDriver(1, 1, 0, .3f);
        yield return character.StartCoroutine(character.layerWeightRoutine);
        yield break;
    }

}

public class DefaultCombatState : CombatState {
    public DefaultCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        try {
            (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).combatMoveSpeed;
        } catch {
            //Debug.Log("not a player");
        }
        yield break;
    }

    // public override IEnumerator OnStateExit() {
    //  //   animator.SetBool(Animator.StringToHash("IsAggro"], false);
    //     animator.SetBool(Animator.StringToHash("WeaponOut"], false);
    //     //Debug.Log("exiting default combat state");
    //     yield break;
    // }

}

public class AttackState : CombatState {
    protected IEnumerator currAttackCoroutine;
    public MeleeMove chosenMove {get; private set;}

    public AttackState(CharacterHandler character, Animator animator) : base(character, animator) {
        try {chosenMove = character.MeleeAttacks["default"]; } catch { Debug.LogWarning("some cunt don't have default attack"); }
    }

    public AttackState(CharacterHandler character, Animator animator, MeleeMove chosenMove) : base(character, animator) {
        this.chosenMove = chosenMove;
    }
    public override IEnumerator OnStateEnter() { 
        animator.SetTrigger(Animator.StringToHash("Attacking"));

        yield return new WaitUntil(() => character.layerWeightRoutine == null);
        character.layerWeightRoutine = character.LayerWeightDriver(1, 1, 0, .3f);
        yield return character.StartCoroutine(character.layerWeightRoutine);
        
        
        currAttackCoroutine = FindTargetAndDealDamage();
        yield return character.StartCoroutine(currAttackCoroutine);
        character.SetStateDriver(new DefaultCombatState(character, animator));
        //animator.ResetTrigger(Animator.StringToHash("Attacking"));
    }    

    protected virtual IEnumerator FindTargetAndDealDamage(){
        CharacterHandler chosenTarget = character.FindTarget(chosenMove);
        
        //animator.applyRootMotion = true;
        //Debug.Log(chosenMove.angle + " " + chosenMove.range);
        //if no targets in range
        if (chosenTarget == null) {
            Array.Find(character.audioData, AudioData => AudioData.name == "woosh").Play(character.AudioSource);
            yield return new WaitForSeconds(chosenMove.startup + chosenMove.endlag); //swing anyways
            character.SetStateDriver(new DefaultCombatState(character, animator));
            yield break;
        }
        //upon completion of finding target/at attack move setup, START listening
        yield return new WaitForSeconds(chosenMove.startup); //assumption: start up == counter window

        chosenTarget.AttackResponse(chosenMove.damage, character);
        //during the endlag phase, check again
        //if I was hit && I am using blockable attack, stagger instead
        yield return new WaitForSeconds(chosenMove.endlag); //TODO if manual endlag
        character.SetStateDriver(new DefaultCombatState(character, animator));
    }

 
    public override IEnumerator OnStateExit() {
        yield return new WaitUntil(() => character.layerWeightRoutine == null);
        character.layerWeightRoutine = character.LayerWeightDriver(1, 0, 1, .3f);
        animator.applyRootMotion = false;
        yield return character.StartCoroutine(character.layerWeightRoutine);       
        if(currAttackCoroutine != null) character.StopCoroutine(currAttackCoroutine); 
        yield break;
    }
}

public class BlockState : CombatState {
    protected MeleeMove block;

    public BlockState(CharacterHandler character, Animator animator) : base(character, animator) {
        try {block = character.MeleeBlock; } catch { Debug.LogWarning("no block in char SO"); }
    }

    public override IEnumerator OnStateEnter() { 
        animator.SetBool(Animator.StringToHash("Blocking"), true);
        yield break;
    } 

    public override IEnumerator OnStateExit() { 
        animator.SetBool(Animator.StringToHash("Blocking"), false);
        yield break;
    } 

}

public class CounterState : CombatState {
    protected MeleeMove block;

    public CounterState(CharacterHandler character, Animator animator) : base(character, animator) {
        try {block = character.MeleeBlock; } catch { Debug.LogWarning("no block in char SO"); }
    }

    public override IEnumerator OnStateEnter() {   
        //trigger counter event
        //if enemy is attacking you specifically (dont trigger if coming from a specific state)
        //shouldnt be done here, should be done in attack response via comparing type
        
        animator.SetBool(Animator.StringToHash("Blocking"), true);
        yield return new WaitForSeconds(0.3f); //where param is counter timeframe
        character.SetStateDriver(new BlockState(character, animator));
    }

    public override IEnumerator OnStateExit() {
        yield break;
    }

}

public class DodgeState : CombatState {
    Vector3 direction;

    public DodgeState(CharacterHandler character, Animator animator, Vector3 direction) : base(character, animator) {
        this.direction = direction;
    }


    public override IEnumerator OnStateEnter() {
        yield return new WaitUntil(() => character.layerWeightRoutine == null);
        character.layerWeightRoutine = character.LayerWeightDriver(1, 1, 0, .8f);
        yield return character.StartCoroutine(character.layerWeightRoutine);  
        animator.ResetTrigger(Animator.StringToHash("Dodging"));
        animator.SetTrigger(Animator.StringToHash("Dodging"));
        yield return new WaitForSeconds((character as PlayerHandler).dodgeTime);
        character.SetStateDriver(new DefaultCombatState(character, animator));
    }

    public override IEnumerator OnStateExit() {
        yield return new WaitUntil(() => character.layerWeightRoutine == null);
        character.layerWeightRoutine = character.LayerWeightDriver(1, 0, 1, .8f);
        yield return character.StartCoroutine(character.layerWeightRoutine);  
    }

}

public class StaggerState : CombatState {
    public StaggerState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {  
        Array.Find(character.audioData, AudioData => AudioData.name == "stagger").Play(character.AudioSource);
        yield return new WaitUntil(() => character.layerWeightRoutine == null);
        character.layerWeightRoutine = character.LayerWeightDriver(1, 1, 0, .2f);
        yield return character.StartCoroutine(character.layerWeightRoutine);       
        animator.SetTrigger(Animator.StringToHash("Staggering"));
        yield return new WaitForSeconds(.5f); //stagger time
        character.SetStateDriver(new DefaultCombatState(character, animator));      
    }

    public override IEnumerator OnStateExit() {
        yield return new WaitUntil(() => character.layerWeightRoutine == null);
        character.layerWeightRoutine = character.LayerWeightDriver(1, 0, 1, .2f);
        yield return character.StartCoroutine(character.layerWeightRoutine);       
    }


}

public class DeathState : CombatState {
    public DeathState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter(){
        Array.Find(character.audioData, AudioData => AudioData.name == "death").Play(character.AudioSource);

        //change globalState
        try{
            (character as AIHandler).GlobalState = GlobalState.DEAD;
            //stop APPROPRITE coroutines
            (character as AIHandler).Detection.IsAlive = false; //detection
            //weapon stuff
            (character as AIHandler).weaponMesh.transform.SetParent(null);
            (character as AIHandler).weaponMesh.AddComponent<Rigidbody>();
            (character as AIHandler).weaponMesh.AddComponent<BoxCollider>();
            //ragdoll 
            (character as AIHandler).GetComponent<Collider>().enabled = false;
        } catch {
            Debug.LogWarning("player shouldnt use death state for now");
        }
        
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

    public override IEnumerator OnStateExit() {
        Debug.Log("exiting death state?");
        yield break;

    }

}