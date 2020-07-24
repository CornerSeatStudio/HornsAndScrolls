using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveItem : MonoBehaviour
{

    private PlayerHandler player;
    
    void OnTriggerEnter(Collider col) {
        if(col.GetComponent<PlayerHandler>()) {
            col.GetComponent<PlayerHandler>().OnInteract += OnPickup;
            player = col.GetComponent<PlayerHandler>();
        }
        
    }

    void OnTriggerExit(Collider col) {
        if(col.GetComponent<PlayerHandler>()) {
            col.GetComponent<PlayerHandler>().OnInteract -= OnPickup;
            player = null;
        }
    }

    public void OnPickup() {
        Debug.Log("picked up");
        //remove it from the scene and parent it to the player
        GetComponent<Collider>().enabled = false;
        gameObject.transform.parent = player.transform;
        gameObject.transform.localPosition = new Vector3(.13f, 5.16f, 2.71f);
        gameObject.transform.localEulerAngles = new Vector3(0f, 0f, -180f);
        gameObject.transform.localScale = new Vector3(47f, 47f, 188f);

        
    }

    
}
