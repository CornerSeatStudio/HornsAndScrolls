using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : CharacterHandler
{
    private IEnumerator currAttackCoroutine;
    
    protected override void Start(){
        base.Start();
        CharacterController cc = GetComponent<CharacterController>();
        cc.center = new Vector3(0,7,0); //temp idk why

    }

    protected override void Update() {
        base.Update(); 

        //sadness is respresented by the length of if else blocks statents
        
        if(combatState is DefaultState) {
            if(Input.GetButtonDown("Fire1") == true) {
                Debug.Log("Fire1'd");
                SetStateDriver(new AttackState(this, animator, MeleeRaycastHandler));
            } else if(Input.GetButtonDown("Fire2") == true) {
                Debug.Log("Fire2'd/counter trigger");
                SetStateDriver(new CounterState(this, animator, MeleeRaycastHandler));
                //counter event is here
            }
     
        } else if (combatState is BlockState) { //gets to here after "counter timer" runs up, aka has to wait for a bit to release block
            //if still blocking
            if(Input.GetButton("Fire2") == true) {
                Debug.Log("holding blocking");
            } else {
                Debug.Log("release block");
                SetStateDriver(new DefaultState(this, animator, MeleeRaycastHandler));
            }
        }
    }


}
