using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationState
{
    protected readonly CharacterHandler character;
    protected Animator animator;

    public AnimationState(CharacterHandler character, Animator animator) {
        this.character = character;
        this.animator = animator;
    }

    public virtual IEnumerator OnStateEnter() {
        yield break;
    }

    public virtual IEnumerator OnStateUpdate() {
        yield break;
    }

    public virtual IEnumerator OnStateExit() {
        yield break;
    }
}
