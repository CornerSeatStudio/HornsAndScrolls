using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastAttackHandler : MonoBehaviour {
    
    public List<GameObject> InteractableTargets {get; } = new List<GameObject>();

    public CharacterHandler chosenTarget {get; private set;}
    private float minDistanceToTarget;

    //LayerMasks allow for raycasts to choose what to and not to register
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public float angle {get; private set; }
    public float range {get; private set; }


public Vector3 DirectionGivenAngle(float angle, bool isGlobal){
        if(!isGlobal){
            angle+=transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle*Mathf.Deg2Rad));
    }

    public bool InAttackCoroutine {get; private set; } = false;
    public IEnumerator FindTargetAndDealDamage(MeleeAttackMove attackMove) { //picks a target in range, then make that character take damage
        angle = attackMove.angle;
        range = attackMove.range;
        
        InAttackCoroutine = true;
        Debug.Log("starting init attack coroutine");
        yield return FindTarget(attackMove);
        yield return new WaitForSeconds(attackMove.startup);
        if (chosenTarget == null) {
            Debug.Log("no targets in range");
            InAttackCoroutine = false;
            yield break;
        }
        chosenTarget.TakeDamage(attackMove.damage);
        yield return new WaitForSeconds(attackMove.endlag);
        InAttackCoroutine = false;
        Debug.Log("end of init attack coroutine - melee has been triggered");
    }

    private IEnumerator FindTarget(MeleeAttackMove attackMove){
        minDistanceToTarget = float.MaxValue; //guarantees first check in findingInteractableTargets
        chosenTarget = null; //reset character chosen
        //cast a sphere over player, store everything inside col
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, attackMove.range, targetMask);
        foreach(Collider col in targetsInView){
            Debug.Log(col.transform);
            Transform target = col.transform; //get the targets locatoin
            Vector3 directionToTarget = (target.position - transform.position).normalized; //direction vector of where bloke is
            if (Vector3.Angle(transform.forward, directionToTarget) < attackMove.angle/2){ //if the FOV is within bounds, /2 cause left right
                //do the ray 
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)){ //if, from character at given angle and distance, it DOESNT collide with obstacleMask
                    //if distance is closer
                    if(distanceToTarget < minDistanceToTarget) {
                        minDistanceToTarget = distanceToTarget;
                        chosenTarget = col.gameObject.GetComponent<CharacterHandler>();
                    }
                }
            }
        }             

        yield break;
    }


}
    

