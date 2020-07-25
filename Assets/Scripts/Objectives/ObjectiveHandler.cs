using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveHandler : MonoBehaviour
{
    
    public List<Objective> objectives;
    public int CurrObjectiveIndex {get; set; } = 0;
    public Objective CurrObjective {get; set; }



    void Start(){
        StartCoroutine(objectives[CurrObjectiveIndex].OnObjectiveStart());
        CurrObjective = objectives[CurrObjectiveIndex];
    }

    public void OnGameFinish() {
        Debug.Log("game end");
    }

    
}
