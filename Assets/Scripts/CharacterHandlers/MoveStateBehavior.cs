using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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