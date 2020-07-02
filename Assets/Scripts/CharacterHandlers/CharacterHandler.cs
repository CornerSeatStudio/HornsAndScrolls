using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHandler : MonoBehaviour
{
    [Header("Core")]
    public CharacterData characterdata;
    public float Health {get; set; }
    //[SerializeField] public float Stamina;
    public HitDetection HitDetection {get; private set; }
    protected AIState localState;
    protected Animator animator;
    //private CharacterController controller;

    //core, callbacks
    protected virtual void Start() {
        HitDetection = this.GetComponent<HitDetection>();
        animator = this.GetComponent<Animator>();
        //controller = this.GetComponent<CharacterController>();
        Health = characterdata.maxHealth;
    }

    public virtual void TakeDamage(float damage){ //probably make this virtual
        Health -= damage;
    }

    //everytime the state is changed, do an exit routine (if applicable), switch the state, then trigger start routine (if applicable)
    public void SetStateDriver(AIState state) { 
        StartCoroutine(SetState(state));
    }

    private IEnumerator SetState(AIState state) {
        if(localState != null) yield return StartCoroutine(localState.OnStateExit());
        localState = state;
        yield return StartCoroutine(localState.OnStateEnter());
    }
    
    

}
