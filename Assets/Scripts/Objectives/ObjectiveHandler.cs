using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ObjectiveHandler : MonoBehaviour
{
    
    public List<Objective> objectives;
    public int CurrObjectiveIndex {get; set; } = 0;
    public Objective CurrObjective {get; set; }
    public Canvas winScreen;

    void Start(){
        winScreen.gameObject.SetActive(false);
        StartCoroutine(objectives[CurrObjectiveIndex].OnObjectiveStart());
        CurrObjective = objectives[CurrObjectiveIndex];
    }

    public void OnGameFinish() {
        Debug.Log("game end");
        winScreen.gameObject.SetActive(true);

    }

    
}
