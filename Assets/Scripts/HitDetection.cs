using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HitDetection : Detection //cause i want fov shit
{

    //these variables should be associated in a scriptable object weapon file
    private CharacterHandler attackTarget;
    public bool InMeleeRoutine {get; set;} = false;

    void Start()
    {        
        StartCoroutine("FindTargetsWithDelay", coroutineDelay); //mono behavior + fov stuff
    }

    //giving end 
    public IEnumerator InitAttack(float startupDelay, float endDelay, float damage){ //initiate any attack - todo: check what attacks are available here
        Debug.Log("starting init attack coroutine");
        InMeleeRoutine = true;
        //as of this point the attack animation should already have begon
        yield return new WaitForSeconds(startupDelay);
        MeleeHit(damage);
        //as of this point the attack animatino should have ended
        yield return new WaitForSeconds(endDelay);
        InMeleeRoutine = false;
        Debug.Log("end of init attack coroutine - melee has been triggered");

    }

    void MeleeHit(float damage) { //interact within given cast - if its an attack, target all in range 
        //read attack
        foreach(GameObject go in InteractableTargets) { //getInteractableTargets depends on assigned layers
            //Debug.Log(go.transform.position);
            attackTarget = go.GetComponent<CharacterHandler>();
            attackTarget.TakeDamage(damage);
        }
        
    }



    void OnCollisionEnter(Collision collider) {
        if(collider.gameObject.tag.Equals("Arrow")){
            Debug.Log("been hit by arrow");
        }
    }


   
}
