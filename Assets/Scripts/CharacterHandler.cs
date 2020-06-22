using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHandler : MonoBehaviour
{
    public CharacterData characterdata;
    [SerializeField] private float health;
    private float stamina;
    protected HitDetection hitDetection;

    [SerializeField] protected UnityEvent<CharacterHandler> onTakeDamage;

    protected virtual void Start() {
        hitDetection = this.GetComponent<HitDetection>();
        health = characterdata.maxHealth;
    }

    public virtual void TakeDamage(float damage){ //probably make this virtual
        health -= damage;
    }

    
    

}
