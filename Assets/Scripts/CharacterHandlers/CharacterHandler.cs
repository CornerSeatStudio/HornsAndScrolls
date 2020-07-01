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
    protected HitDetection hitDetection;
    private AnimationState localState;
    private Animator animator;

    //core, callbacks
    protected virtual void Start() {
        hitDetection = this.GetComponent<HitDetection>();
        animator = this.GetComponent<Animator>();
        Health = characterdata.maxHealth;
    }

    public virtual void TakeDamage(float damage){ //probably make this virtual
        Health -= damage;
    }

    public void LateUpdate(){
        //if cunt be walking, walk
        if(transform.hasChanged) {
            SetAnimationStateDriver(new WalkingState(this, animator));
        } else {
            SetAnimationStateDriver(new IdleState(this, animator));
        }
    }

    //everytime the state is changed, do an exit routine (if applicable), switch the state, then trigger start routine (if applicable)
    public void SetAnimationStateDriver(AnimationState state) { 
        StartCoroutine(SetAnimationState(state));
    }

    private IEnumerator SetAnimationState(AnimationState state) {
        if(localState != null) yield return StartCoroutine(localState.OnStateExit());
        localState = state;
        yield return StartCoroutine(localState.OnStateEnter());
    }
    
    

}
