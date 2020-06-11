using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HitDetection : Detection //cause i want fov shit
{

    //these variables should be associated in a scriptable object weapon file
    public float startupDelay = .2f;
    public float endDelay = .2f; 
    private IEnumerator currentInitAttackCoroutine;

    void Start()
    {        
        StartCoroutine("FindTargetsWithDelay", coroutineDelay); //mono behavior + fov stuff
        
    }

    void Update() {
        //UnityEngine.Debug.Log("test");
        if(Input.GetButtonDown("Fire1") == true) { //to be moved in own inherited class (respective to AI or player)

            if(currentInitAttackCoroutine == null) { //if coroutine hasnt finshed (either allow interuppts OR force an attack to finish its animation as is now)
                currentInitAttackCoroutine = InitAttack(startupDelay);
                StartCoroutine(currentInitAttackCoroutine);
            }
            
        }

    }

    //giving end 
    IEnumerator InitAttack(float startupDelay){ //initiate any attack - todo: check what attacks are available here
        //as of this point the attack animation should already have begon
        yield return new WaitForSeconds(startupDelay);
        MeleeHit();
        //as of this point the attack animatino should have ended
        yield return new WaitForSeconds(endDelay);
        currentInitAttackCoroutine = null; //allow for another attack
    }

    void MeleeHit() { //interact within given cast - if its an attack, target all in range (maybe make enum for more options)
        //read attack
        foreach(GameObject go in getInteractableTargets()) {
                Debug.Log(go.transform.position);
        }
    }


    //receiving end
    void OnCollisionEnter(Collision collider) {
        if(collider.gameObject.tag.Equals("Arrow")){
            Debug.Log("been hit by arrow");
        }
    }


   
}
