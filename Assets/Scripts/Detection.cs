using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    [Range(0, 360)] public float viewAngle = 30;
    public float viewRadius = 10;
    public float interactionDistance = 3;
    public float coroutineDelay = .2f;

    //LayerMasks allow for raycasts to choose what to and not to register
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> VisibleTargets {get; } = new List<Transform>();
    public List<GameObject> InteractableTargets {get; } = new List<GameObject>();

    void Start() {
        StartCoroutine("FindTargetsWithDelay", coroutineDelay);
    } 

    private IEnumerator FindTargetsWithDelay(float delay){
        while (true){
            yield return new WaitForSeconds(delay); //only coroutine every delay seconds
            findVisibleTargets();
        }
    }


    //for every target (via an array), lock on em
    private void findVisibleTargets(){
        VisibleTargets.Clear(); //reset list every time so no dupes
        InteractableTargets.Clear();
        //cast a sphere over player, store everything inside col
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        foreach(Collider col in targetsInView){
            Transform target = col.transform; //get the targets locatoin
            Vector3 directionToTarget = (target.position - transform.position).normalized; //direction vector of where bloke is
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle/2){ //if the FOV is within bounds, /2 cause left right
                //do the ray 
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)){ //if, from character at given angle and distance, it DOESNT collide with obstacleMask
                    VisibleTargets.Add(target);
                    //Debug.Log("spot a cunt");
                    if(distanceToTarget <= interactionDistance) { //additional check if in range of an interaction
                        InteractableTargets.Add(col.gameObject);
                        //Debug.Log("interactable distance satisfied");
                    }
                }
            }
        }

    }

    
    //return direction vector given a specific angle
    public Vector3 directionGivenAngle(float angle, bool isGlobal){
        if(!isGlobal){
            angle+=transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle*Mathf.Deg2Rad));
    }

}
