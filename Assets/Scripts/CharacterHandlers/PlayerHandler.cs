using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : CharacterHandler
{

    public WeaponData weapon; //Todo: list to choose between

    
    public enum PlayerState { HIDDEN, COMBAT, DEAD };

    
    void Update() {
        if(Input.GetButtonDown("Fire1") == true) {
            Debug.Log("Fire1'd, inAttackCoroutine?: " + attackHandler.InAttackCoroutine);
            if(!attackHandler.InAttackCoroutine){
                StartCoroutine(attackHandler.FindTargetAndDealDamage(weapon.MeleeAttackMoves[0]));
            }
        }
        
    }
    

}
