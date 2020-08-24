using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveState : GenericState { //represents the current MOVEMENT status of a character
    
    protected readonly CharacterHandler character;
    protected Animator animator;

    public MoveState(CharacterHandler character, Animator animator) {
        this.character = character;
        this.animator = animator;
    }

    public abstract IEnumerator OnStateEnter();

    public abstract IEnumerator OnStateExit();

}
