using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

//send an event per stance change onStateEnter
//listened in detection script

public class IdleMoveState : MoveState {
    public IdleMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).ChangeStanceTimer((character.characterdata as PlayerData).detectionTime);
        animator.SetBool(Animator.StringToHash("Crouching"), false);

   //     animator.SetBool(Animator.StringToHash("Idle"), true);
        yield break;
    }

    public override IEnumerator OnStateExit() {
       // animator.SetBool(Animator.StringToHash("Idle"), false);
        yield break;    
    }
}

public class JogMoveState : MoveState {
    public JogMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).ChangeStanceTimer((character.characterdata as PlayerData).detectionTime /2);
        animator.SetBool(Animator.StringToHash("Crouching"), false);

       // animator.SetBool(Animator.StringToHash("Jogging"), true);
        (character as PlayerHandler).CurrMovementSpeed = (character.characterdata as PlayerData).jogSpeed;
        yield break;    
    
    }

    public override IEnumerator OnStateExit() {
        //animator.SetBool(Animator.StringToHash("Jogging"), false);
        yield break;    
    }
}

public class SprintMoveState : MoveState {
    IEnumerator StaminaDrain;

    public SprintMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).ChangeStanceTimer((character.characterdata as PlayerData).detectionTime /2);
        animator.SetBool(Animator.StringToHash("Crouching"), false);
       // animator.SetBool(Animator.StringToHash("Sprinting"), true);
        (character as PlayerHandler).CurrMovementSpeed = (character.characterdata as PlayerData).sprintSpeed;
        StaminaDrain = DrainStaminaOverTime();
        character.StartCoroutine(StaminaDrain);
        yield return new WaitUntil(()=>character.Stamina <= 0);
        character.SetStateDriver(new JogMoveState(character, animator));
    }

    private IEnumerator DrainStaminaOverTime() {
        while (character.Stamina > 0) {
            character.DealStamina(.4f);
            yield return new WaitForSeconds(.1f);
        }
    }

    public override IEnumerator OnStateExit() {
        if(StaminaDrain != null) character.StopCoroutine(StaminaDrain);
        //animator.SetBool(Animator.StringToHash("Sprinting"), false);
        yield break;    
    }
}

public class WalkMoveState : MoveState {
    public WalkMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).ChangeStanceTimer((character.characterdata as PlayerData).detectionTime);
        animator.SetBool(Animator.StringToHash("Crouching"), false);

       // animator.SetBool(Animator.StringToHash("Walking"), true);
        (character as PlayerHandler).CurrMovementSpeed = (character.characterdata as PlayerData).walkSpeed;
        yield break;    
    }

    public override IEnumerator OnStateExit() {
       // animator.SetBool(Animator.StringToHash("Walking"), false);
        yield break;    
    }
}

public class CrouchIdleMoveState : MoveState {
    public CrouchIdleMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).ChangeStanceTimer((character.characterdata as PlayerData).detectionTime * 2.5f);
        animator.SetBool(Animator.StringToHash("Crouching"), true);
        yield break;    
    }

    public override IEnumerator OnStateExit() {
      //  animator.SetBool(Animator.StringToHash("Crouching"), false);

        yield break;    
    }
}

public class CrouchWalkMoveState : MoveState {
    public CrouchWalkMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).ChangeStanceTimer((character.characterdata as PlayerData).detectionTime * 2.3f);
        animator.SetBool(Animator.StringToHash("Crouching"), true);
        (character as PlayerHandler).CurrMovementSpeed = (character.characterdata as PlayerData).crouchWalkSpeed;        
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        yield break;    
    }
}