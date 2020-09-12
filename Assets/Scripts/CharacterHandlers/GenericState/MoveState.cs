using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//represents the current MOVEMENT status of a character
//this is used exclusively by player handlers (which probably is why the constructur's character SHOULD BE a playerhandler, not a characterhandler), too lazy to change
//CombatState and MoveState are switched between in the PlayerHandler

public abstract class MoveState : GenericState { 
    
    protected readonly CharacterHandler character;
    protected Animator animator;

    public MoveState(CharacterHandler character, Animator animator) {
        this.character = character;
        this.animator = animator;
    }

    public abstract IEnumerator OnStateEnter();

    public abstract IEnumerator OnStateExit();

}
