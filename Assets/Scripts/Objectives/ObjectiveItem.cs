using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class ObjectiveItem : Objective
{
    private PlayerHandler player;
    public GameObject onPlayerPrefab;
    
    public void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size);
    }

    void OnTriggerEnter(Collider col) {
        if(col.GetComponent<PlayerHandler>() && objectiveHandler.CurrObjective == this) {
            player = col.GetComponent<PlayerHandler>();
            player.OnInteract += OnPickup;
           
        }
    } 

    void OnTriggerExit(Collider col) {
        if(col.GetComponent<PlayerHandler>() && objectiveHandler.CurrObjective == this) {
            player.OnInteract -= OnPickup;
            player = null;
        }
    }


    public void OnPickup() {


        //remove from listener
        player.OnInteract -= OnPickup;

        //grab item placeholder thing (if exists)
        if(onPlayerPrefab != null) onPlayerPrefab.SetActive(true);

        StartCoroutine(OnObjectiveIncrement());
        
        //remove it from the scene
        GetComponent<Collider>().enabled = false;
        
        this.gameObject.SetActive(false);

    }

    
}
