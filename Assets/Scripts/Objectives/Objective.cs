using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public abstract class Objective : MonoBehaviour
{

    protected ObjectiveHandler objectiveHandler;
    public UnityEvent onObjectiveCompletion;
    protected virtual void Start(){
        //locate obectiveHandler
        objectiveHandler = GameObject.FindObjectOfType<ObjectiveHandler>();
        objectiveHandler.CurrObjectiveIndex = 0; //start from beginning
        GetComponent<Collider>().isTrigger = true;
    }

    //reset to the beginning 
    protected IEnumerator OnObjectiveReset() { //this should be called if the player dies
        yield return StartCoroutine(objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex].OnObjectiveFailure());
        objectiveHandler.CurrObjectiveIndex = 0;
        objectiveHandler.CurrObjective = objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex];
        yield return StartCoroutine(objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex].OnObjectiveStart());
    }

    protected IEnumerator OnObjectiveIncrement() { //this should be called if an objective is completed
        

        StartCoroutine(objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex].OnObjectiveCompletion());
        objectiveHandler.CurrObjectiveIndex++;


        if(objectiveHandler.objectives.Count > objectiveHandler.CurrObjectiveIndex) {
            objectiveHandler.CurrObjective = objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex];
            yield return StartCoroutine(objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex].OnObjectiveStart());
        } else {
            Debug.Log("objectives done");
            objectiveHandler.OnLevelFinish();
        }
    }

    public virtual IEnumerator OnObjectiveStart() {
        //Debug.Log("default objective start");
        try{Array.Find(objectiveHandler.audioData, AudioData => AudioData.name == "win").Play(objectiveHandler.AudioSource);}catch{}

        yield break;
    }

    public virtual IEnumerator OnObjectiveCompletion() {
        //Debug.Log("default objective completion");
        Array.Find(objectiveHandler.audioData, AudioData => AudioData.name == "win").Play(objectiveHandler.AudioSource);

        onObjectiveCompletion?.Invoke();

        yield break;
    }

    public virtual IEnumerator OnObjectiveFailure() {
        //Debug.Log("default objective failure");
        yield break;
    }

    
}
