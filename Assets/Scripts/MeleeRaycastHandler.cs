using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRaycastHandler : MonoBehaviour {
    
    public CharacterHandler chosenTarget {get; set;}
    private float minDistanceToTarget;

    //LayerMasks allow for raycasts to choose what to and not to register
    public LayerMask targetMask;
    public LayerMask obstacleMask;
 
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
    

