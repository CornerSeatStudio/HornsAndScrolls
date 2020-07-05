﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : CharacterHandler
{
    private IEnumerator currAttackCoroutine;
    
    protected override void Start(){
        base.Start();

    }

    void Update() {
        //sadness is respresented by the length of if else blocks statents
        
        if(combatState is DefaultState) {
            if(Input.GetButtonDown("Fire1") == true) {
                Debug.Log("Fire1'd");
                SetStateDriver(new AttackState(this, animator, meleeRaycastHandler));
            } else if(Input.GetButtonDown("Fire2") == true) {
                Debug.Log("Fire2'd/counter trigger");
                SetStateDriver(new BlockState(this, animator, meleeRaycastHandler));
                //counter event is here
            }
     
        } else if (combatState is BlockState) {
            //if still blocking
            if(Input.GetButton("Fire2") == true) {
                Debug.Log("holding blocking");
            } else {
                Debug.Log("release block");
                SetStateDriver(new DefaultState(this, animator, meleeRaycastHandler));
            }
        }
    }


}
