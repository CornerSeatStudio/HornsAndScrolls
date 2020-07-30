using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public abstract class Objective : MonoBehaviour
{

    protected ObjectiveHandler objectiveHandler;

    protected virtual void Start(){
        //locate obectiveHandler
        objectiveHandler = GameObject.FindObjectOfType<ObjectiveHandler>();
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
        yield return StartCoroutine(objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex].OnObjectiveCompletion());
        objectiveHandler.CurrObjectiveIndex++;

        if(objectiveHandler.objectives.Count > objectiveHandler.CurrObjectiveIndex) {
            objectiveHandler.CurrObjective = objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex];
            yield return StartCoroutine(objectiveHandler.objectives[objectiveHandler.CurrObjectiveIndex].OnObjectiveStart());
        } else {
            Debug.Log("objectives done");
            objectiveHandler.OnGameFinish();
        }
    }

    public virtual IEnumerator OnObjectiveStart() {
        //Debug.Log("default objective start");
        Array.Find(objectiveHandler.audioData, AudioData => AudioData.name == "win").Play(objectiveHandler.AudioSource);

        yield break;
    }

    public virtual IEnumerator OnObjectiveCompletion() {
        //Debug.Log("default objective completion");
        Array.Find(objectiveHandler.audioData, AudioData => AudioData.name == "win").Play(objectiveHandler.AudioSource);

        yield break;
    }

    public virtual IEnumerator OnObjectiveFailure() {
        //Debug.Log("default objective failure");
        yield break;
    }

    
}
