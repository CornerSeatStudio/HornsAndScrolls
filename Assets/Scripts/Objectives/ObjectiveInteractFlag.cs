using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ObjectiveInteractFlag : Objective
{

    private PlayerHandler player;

    public void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size);
    }
    void OnTriggerEnter(Collider col) {
        if(col.GetComponent<PlayerHandler>() && objectiveHandler.CurrObjective == this) {
            player = col.GetComponent<PlayerHandler>();
            player.OnInteract += OnDo;
           
        }
    } 

    void OnTriggerExit(Collider col) {
        if(col.GetComponent<PlayerHandler>() && objectiveHandler.CurrObjective == this) {
            player.OnInteract -= OnDo;
            player = null;
        }
    }

    public void OnDo() {
        //remove from listener
        player.OnInteract -= OnDo;
        StartCoroutine(OnObjectiveIncrement());

    }
}
