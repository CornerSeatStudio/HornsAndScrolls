using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//this represents when a character is in a combat mode. 
//CombatState and MoveState are switched between in the PlayerHandler
public abstract class CombatState : GenericState {
    protected readonly CharacterHandler character;
    protected Animator animator;

    public CombatState(CharacterHandler character, Animator animator) {
        this.character = character;
        this.animator = animator;
    }

    public virtual IEnumerator OnStateEnter() {
        yield break;
    }

    public virtual IEnumerator OnStateExit() {
        yield break;
    }
    
}
