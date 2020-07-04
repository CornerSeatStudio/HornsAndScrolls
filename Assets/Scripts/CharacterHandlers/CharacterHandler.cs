using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHandler : MonoBehaviour
{
    [Header("Core")]
    public CharacterData characterdata;
    public float Health {get; set; }
    protected AIState localState;
    protected Animator animator;
    protected RaycastAttackHandler attackHandler;

    #region Callbacks
    protected virtual void Start() {
        animator = this.GetComponent<Animator>();
        attackHandler = this.GetComponent<RaycastAttackHandler>();
        Health = characterdata.maxHealth;
    }
    #endregion

    public virtual void TakeDamage(float damage){ 
        Health -= damage;
    }


    #region FSM
    //everytime the state is changed, do an exit routine (if applicable), switch the state, then trigger start routine (if applicable)
    public void SetStateDriver(AIState state) { 
        StartCoroutine(SetState(state));
    }

    private IEnumerator SetState(AIState state) {
        if(localState != null) yield return StartCoroutine(localState.OnStateExit());
        localState = state;
        yield return StartCoroutine(localState.OnStateEnter());
    }
    
    #endregion

}
