using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))] //cause hit registry requires colliders
public class ToLevelSelectFlag : MonoBehaviour
{

    private SaveManager saveManager;

    public void Start(){
        saveManager = this.GetComponent<SaveManager>();
    }

    public void OnTriggerEnter(Collider col){
        if(col.GetComponent<PlayerHandler>()) saveManager.OnLevelSelectLoad();
    }
    
}
