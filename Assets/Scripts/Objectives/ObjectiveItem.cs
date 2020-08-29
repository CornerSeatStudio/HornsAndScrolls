using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class ObjectiveItem : Objective
{
    private PlayerHandler player;
    public Transform itemAttachTransform;
    public Animator animator;
    public Animator otherdoor;
    public ObjectiveObject item;
    
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

        //remove it from the scene and parent it to the player (temporary for horn)
        GetComponent<Collider>().enabled = false;
    
        transform.parent = itemAttachTransform;
        transform.localPosition = item.pickupPosition;
        transform.localEulerAngles = item.pickupEulerAngle;
        transform.localScale = item.pickupScale;
        animator.SetBool("IsOpen",true);
        otherdoor.SetBool("IsOpens",true);
        StartCoroutine(OnObjectiveIncrement());
    }

    
}
