using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ObjectiveFlag : Objective
{
    public void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size);
    }

    void OnTriggerEnter(Collider col) {
        if(col.GetComponent<PlayerHandler>() && objectiveHandler.CurrObjective == this) StartCoroutine(OnObjectiveIncrement());
        GetComponent<Collider>().enabled = false; 
    } 

}
