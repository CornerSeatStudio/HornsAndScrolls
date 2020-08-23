using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for items around the world, NOT INVENTORY STUFF
public class Item : MonoBehaviour
{
    PlayerHandler playerRef;
    public ItemObject itemObject;
    
    public void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size);
    }

    void OnTriggerEnter(Collider col) {
        if(col.GetComponent<PlayerHandler>()) {
            playerRef = col.GetComponent<PlayerHandler>();
            playerRef.OnInteract += OnPickup;
           
        }
        
    } 

    void OnTriggerExit(Collider col) {
        if(col.GetComponent<PlayerHandler>()) {
            playerRef.OnInteract -= OnPickup;
            playerRef = null;
        }
    }

    //picking it up in the world
    void OnPickup(){
        //remove from listener, disable collider
        playerRef.OnInteract -= OnPickup;
        GetComponent<Collider>().enabled = false;

        Destroy(this); //destruct
    }
}

/*
ItemObject itemInQuestion
PlayerHandler playerReference
OnTriggerEnter() 
Grab player reference
Add to OnInteractEvent
Highlight icon maybe, sprite stuff idk
If interaction button pressed, call OnPickup()
OnTriggerExit()
Remove from OnInteract event
Nullify player reference for safety
OnPickup()
Remove from listener
Add to the player inventory (via playerReference)
disable collider
itemInQuetion.OnPickup()
*/