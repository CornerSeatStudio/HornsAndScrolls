using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ObjectiveHandler : MonoBehaviour
{
    
    public List<Objective> objectives;
    public int CurrObjectiveIndex {get; set; } = 0;
    public Objective CurrObjective {get; set; }
    public Canvas winScreen;
    public AudioData[] audioData;
    public AudioSource AudioSource {get; private set;}
    private SaveManager saveManager;


    void Start(){
        saveManager = FindObjectOfType<SaveManager>();
        AudioSource = this.GetComponent<AudioSource>();
        winScreen.gameObject.SetActive(false);
        StartCoroutine(objectives[CurrObjectiveIndex].OnObjectiveStart());
        CurrObjective = objectives[CurrObjectiveIndex];

       // Debug.Log($"current level SHOULD BE: {SaveManager.CurrentLevelIndex}");
    }

    public void OnLevelFinish() {
        StartCoroutine(OnGameFinishWithTime());
    }

    IEnumerator OnGameFinishWithTime(){
        //game end (maybe a fade out?)
        Debug.Log("game end");
        winScreen.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);

        //save, then load the next scene
       // saveManager.OnStandardLevelCompletion();
    }

    
}
