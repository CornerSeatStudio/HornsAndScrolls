using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    public Animator door1;
    public Animator door2;
    
    Vector3 oldCamPos;
    Quaternion oldCamRot;

    public void OnDoorActivate(){
        StartCoroutine(DoorTimings());

    }

    IEnumerator DoorTimings(){
        //snap camera, save old. freeze stuff
        oldCamPos = Camera.main.transform.position;
        oldCamRot = Camera.main.transform.rotation;
        Camera.main.GetComponent<CameraHandler>().enabled = false;


        //camera at em
        Camera.main.transform.position = new Vector3(170, 71, -245);
        Camera.main.transform.rotation = Quaternion.Euler(new Vector3(40, 0.67f, 0.43f));

        //play animations
        door1.SetTrigger(Animator.StringToHash("open"));
        door2.SetTrigger(Animator.StringToHash("open"));

        yield return new WaitForSeconds(2f);

        //camera back
        Camera.main.GetComponent<CameraHandler>().enabled = true;
        Camera.main.transform.position = oldCamPos;
        Camera.main.transform.rotation = oldCamRot;


    }
}
