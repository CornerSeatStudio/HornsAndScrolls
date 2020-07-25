using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SheathCrouchState : MoveState {
    IEnumerator sheathRoutine;

    public SheathCrouchState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        sheathRoutine = Sheath();
        yield return character.StartCoroutine(sheathRoutine);
        character.SetStateDriver(new CrouchIdleMoveState(character, animator));

    }

    private IEnumerator Sheath(){ 
        animator.SetBool(Animator.StringToHash("WeaponOut"), false); 
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


public class IdleMoveState : MoveState {
    public IdleMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        animator.SetBool(Animator.StringToHash("Idle"), true);
        yield break;
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(Animator.StringToHash("Idle"), false);
        yield break;    
    }
}

public class JogMoveState : MoveState {
    public JogMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(Animator.StringToHash("Jogging"), true);
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).jogSpeed;
        yield break;    
    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(Animator.StringToHash("Jogging"), false);
        yield break;    
    }
}

public class SprintMoveState : MoveState {
    public SprintMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(Animator.StringToHash("Sprinting"), true);
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).sprintSpeed;
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(Animator.StringToHash("Sprinting"), false);
        yield break;    
    }
}

public class WalkMoveState : MoveState {
    public WalkMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(Animator.StringToHash("Walking"), true);
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).walkSpeed;
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(Animator.StringToHash("Walking"), false);
        yield break;    
    }
}

public class CrouchIdleMoveState : MoveState {
    public CrouchIdleMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(Animator.StringToHash("Crouching"), true);
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(Animator.StringToHash("Crouching"), false);

        yield break;    
    }
}

public class CrouchWalkMoveState : MoveState {
    public CrouchWalkMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(Animator.StringToHash("CrouchWalking"), true);
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).crouchWalkSpeed;        
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(Animator.StringToHash("CrouchWalking"), false);
        yield break;    
    }
}