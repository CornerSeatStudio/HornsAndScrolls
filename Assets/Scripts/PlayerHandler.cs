using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : CharacterHandler
{

    public float startupDelay = .2f;
    public float endDelay = .2f; 
    public float damage = 3f;

    
    private IEnumerator currentAttackCoroutine;

    
    void Update() {
        currentAttackCoroutine = hitDetection.currentInitAttackCoroutine; //always to check for active coroutines

        if(Input.GetButtonDown("Fire1") == true) {
            if(currentAttackCoroutine == null) { //if coroutine hasnt finshed (either allow interuppts OR force an attack to finish its animation as is now)
                currentAttackCoroutine = hitDetection.InitAttack(startupDelay, endDelay, damage);
                StartCoroutine(currentAttackCoroutine);
            }
        }
        
    }

    

}
