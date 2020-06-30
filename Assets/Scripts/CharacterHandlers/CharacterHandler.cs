using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CharacterLocalState {STANDING, JUMPING, CROUCHING, RUNNING, WALKING}
public class CharacterHandler : MonoBehaviour
{
    [Header("Core")]
    public CharacterData characterdata;
    [SerializeField] public float Health {get; set; }
    //[SerializeField] public float Stamina;
    protected HitDetection hitDetection;
    private CharacterLocalState localState;
    protected virtual void Start() {
        hitDetection = this.GetComponent<HitDetection>();
        Health = characterdata.maxHealth;
    }

    public void LateUpdate() {
        switch (localState) {
            case CharacterLocalState.WALKING :
                break;
            case CharacterLocalState.CROUCHING :
                break;
            case CharacterLocalState.RUNNING :
                break;
            default:
                break;
        }
    }

    public virtual void TakeDamage(float damage){ //probably make this virtual
        Health -= damage;
    }

    public void onDeath() {
        //stop detection coroutine
        //play death animation

        //AI specific:
        //set combat status to dead, 

    }
    
    

}
