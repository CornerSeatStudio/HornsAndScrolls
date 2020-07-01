using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : AnimationState {
    public IdleState(CharacterHandler character, Animator animator) : base(character, animator) {}
    
    public override IEnumerator OnStateEnter() {
        animator.SetBool("IsIdle", true);
        yield return null;
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool("IsIdle", false);
        yield return null;
    }
}

public class WalkingState : AnimationState {
    public WalkingState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        animator.SetBool("IsWalking", true);
        yield return null;
    }

    public override IEnumerator OnStateExit() {
        animator.SetBool("IsWalking", false);
        yield return null;
    }
}