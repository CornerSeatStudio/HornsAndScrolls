﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHandler : MonoBehaviour
{
    [Header("Core")]
    public CharacterData characterdata;
    [SerializeField] public float Health {get; set; }
    //[SerializeField] public float Stamina;
    protected HitDetection hitDetection;
    protected virtual void Start() {
        hitDetection = this.GetComponent<HitDetection>();
        Health = characterdata.maxHealth;
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