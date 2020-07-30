using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class ObjectiveItem : Objective
{

    private PlayerHandler player;

    public Transform itemAttachTransform;

    public Vector3 pickupPosition;
    public Vector3 pickupEulerAngle;
    public Vector3 pickupScale;
    
    public void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(GetComponent<BoxCollider>().bounds.center, GetComponent<BoxCollider>().bounds.size);
    }

    void OnTriggerEnter(Collider col) {
        if(col.GetComponent<PlayerHandler>() && objectiveHandler.CurrObjective == this) {
            col.GetComponent<PlayerHandler>().OnInteract += OnPickup;
            player = col.GetComponent<PlayerHandler>();
        }
        
    } 

    void OnTriggerExit(Collider col) {
        if(col.GetComponent<PlayerHandler>() && objectiveHandler.CurrObjective == this) {
            col.GetComponent<PlayerHandler>().OnInteract -= OnPickup;
            player = null;
        }
    }


    public void OnPickup() {
        Debug.Log("picked up");

        //remove from listener
        player.OnInteract -= OnPickup;

        //remove it from the scene and parent it to the player (temporary for horn)
        GetComponent<Collider>().enabled = false;
    

        if(itemAttachTransform != null) {
            gameObject.transform.parent = itemAttachTransform;
            gameObject.transform.localPosition = pickupPosition;
            gameObject.transform.localEulerAngles = pickupEulerAngle;
            gameObject.transform.localScale = pickupScale;
        } else {
            //or make it dissapear
            //gameObject.SetActive(false);
        }

        // gameObject.transform.localPosition = new Vector3(.01f, 0.01f, 0.006f);
        // gameObject.transform.localEulerAngles = new Vector3(180f, 0-112, -16f);
        // gameObject.transform.localScale = new Vector3(.3f, .3f, 1.2f);

        StartCoroutine(OnObjectiveIncrement());
    }

    
}
