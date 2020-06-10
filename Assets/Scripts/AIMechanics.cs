using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMechanics : FieldOfView //cause i want fov shit
{

    [SerializeField] private Transform destination;
    private NavMeshAgent agent;
    void Start()
    {        
        StartCoroutine("FindTargetsWithDelay", coroutineDelay); //mono behavior + fov stuff
        agent = this.GetComponent<NavMeshAgent>();
        //SetDestination();
    }

    void OnCollisionEnter(Collision collider) {
        if(collider.gameObject.tag.Equals("Arrow")){
            Debug.Log("been hit by arrow");
        }
    }

    private void SetDestination(){
        if(destination != null){
            Vector3 targetVector = destination.transform.position;
            agent.SetDestination(targetVector);
        }
    }

   
}
