using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRaycastHandler : MonoBehaviour {
    
    public List<GameObject> InteractableTargets {get; } = new List<GameObject>();

    public CharacterHandler chosenTarget {get; set;}
    private float minDistanceToTarget;

    //LayerMasks allow for raycasts to choose what to and not to register
    public LayerMask targetMask;
    public LayerMask obstacleMask;
    
    public IEnumerator FindTargetAndDealDamage(MeleeMove meleeMove) { //picks a target in range, then make that character take damage

        Debug.Log("starting init attack coroutine");
        yield return FindTarget(meleeMove);
        //if no targets in range
        if (chosenTarget == null) {
            Debug.Log("no targets in range");
            yield return new WaitForSeconds(meleeMove.startup + meleeMove.endlag); //swing anyways
        }

        //upon completion of finding target/at attack move setup, START listening
        yield return new WaitForSeconds(meleeMove.startup); //assumption: start up == counter window

        //as of this moment, there should be information on 
        //if the character is blocking or was hit in the middle of the attack
        //additionally, if the player countered or dodged, 
        
        //take LAST PERCIEVED EVENT

        //todo: put in respective classes (replace/add to TakeDamage method)

        //attacking anyone
        //if character is blocking && I am using blockable attack, dont do damage but finish animation - global flag toggle
        //if I was hit && I am using blockable attack, stagger instead

        //attacking Player
        //if i, AI, was hit && i, AI, am using unblockable attack, damage to player as usual
        //if player is blocking && i, AI, am using unblockable attack, deal damage still
        //if player is countering && i, AI, am using unblockable attack, act like blockable attack
        //if player is countering && i, AI, am using blockable attack, begin stagger animation, yield return slower maybe?
        //if player dodges, do no damage/nothing

        //if all else fails, deal the damage
        chosenTarget.TakeDamage(meleeMove.damage);

        //during the endlag phase, check again
        //if I was hit && I am using blockable attack, stagger instead
        yield return new WaitForSeconds(meleeMove.endlag);


        Debug.Log("end of init attack coroutine - melee has been triggered");
    }

    public IEnumerator FindTarget(MeleeMove meleeMove){
        minDistanceToTarget = float.MaxValue; //guarantees first check in findingInteractableTargets
        chosenTarget = null; //reset character chosen
        //cast a sphere over player, store everything inside col
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, meleeMove.range, targetMask);
        foreach(Collider col in targetsInView){
            //Debug.Log(col.transform);
            Transform target = col.transform; //get the targets locatoin
            Vector3 directionToTarget = (target.position - transform.position).normalized; //direction vector of where bloke is
            if (Vector3.Angle(transform.forward, directionToTarget) < meleeMove.angle/2){ //if the FOV is within bounds, /2 cause left right
                //do the ray 
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)){ //if, from character at given angle and distance, it DOESNT collide with obstacleMask
                    //if distance is closer
                    if(distanceToTarget < minDistanceToTarget) {
                        minDistanceToTarget = distanceToTarget;
                        chosenTarget = col.GetComponent<CharacterHandler>();
                    }
                }
            }
        }             

        yield break;
    }


}
    

