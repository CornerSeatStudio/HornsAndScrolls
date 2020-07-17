using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleMoveState : MoveState {
    public IdleMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        animator.SetBool(character.AnimationHashes["Idle"], true);
        yield break;
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(character.AnimationHashes["Idle"], false);
        yield break;    
    }
}

public class JogMoveState : MoveState {
    public JogMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(character.AnimationHashes["Jogging"], true);
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).jogSpeed;
        yield break;    
    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(character.AnimationHashes["Jogging"], false);
        yield break;    
    }
}

public class SprintMoveState : MoveState {
    public SprintMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(character.AnimationHashes["Sprinting"], true);
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).sprintSpeed;
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(character.AnimationHashes["Sprinting"], false);
        yield break;    
    }
}

public class WalkMoveState : MoveState {
    public WalkMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(character.AnimationHashes["Walking"], true);
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).walkSpeed;
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(character.AnimationHashes["Walking"], false);
        yield break;    
    }
}

public class CrouchIdleMoveState : MoveState {
    public CrouchIdleMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(character.AnimationHashes["Crouching"], true);
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(character.AnimationHashes["Crouching"], false);

        yield break;    
    }
}

public class CrouchWalkMoveState : MoveState {
    public CrouchWalkMoveState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool(character.AnimationHashes["CrouchWalking"], true);
        (character as PlayerHandler).CurrMovementSpeed = (character as PlayerHandler).crouchWalkSpeed;        
        yield break;    
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool(character.AnimationHashes["CrouchWalking"], false);
        yield break;    
    }
}