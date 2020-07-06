using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class CombatState
{
    protected readonly CharacterHandler character;
    protected Animator animator;
    protected MeleeRaycastHandler attackHandler;

    public CombatState(CharacterHandler character, Animator animator, MeleeRaycastHandler attackHandler) {
        this.character = character;
        this.animator = animator;
        this.attackHandler = attackHandler;
    }

    public virtual IEnumerator OnStateEnter() {
        yield break;
    }

    public virtual IEnumerator OnStateExit() {
        yield break;
    }
    
}
