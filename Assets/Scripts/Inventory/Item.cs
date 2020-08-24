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

        //PUT IT IN INVENTORY
        playerRef.inventory.AddItem(itemObject);

        Destroy(this.gameObject); //destruct
    }
}

