using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class SaveManager : MonoBehaviour
{

    public void Update(){
        if(Input.GetKeyDown(KeyCode.P)) {
            Debug.Log(CurrentLevelIndex);
        }

     
    }

    public void Start(){
        //aka if on main menu
        if(SceneManager.GetActiveScene().buildIndex == 0) {
            OnLoad();
        }
    }

    //saving stuff
    public static int CurrentLevelIndex {get; private set;}

    public void OnLoad(){
        CurrentLevelIndex = Saving.Load().currentLevelIndex;

    }

    public void OnSave() {
        Saving.Save(this);
    }

    public void ResetData() {
        CurrentLevelIndex = 0;
    }

    public void OnLevelCompletion(){
       // Debug.Log("saved?");

        //update completed scene value
        CurrentLevelIndex = Mathf.Max(CurrentLevelIndex, SceneManager.GetActiveScene().buildIndex + 1);

       //save it
        Saving.Save(this);
        //move to next scene
        IncrementScene();
    }

    //load most recent scene
    public void OnContinue(){
        if(CurrentLevelIndex == 0) OnCleanStart();
        SceneManager.LoadScene(CurrentLevelIndex);
    }

    //start from the beginning (without reseting data)
    public void OnCleanStart(){
        CurrentLevelIndex = 1;
        IncrementScene();
    }
    //goes to next scene
    private void IncrementScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void DecrementScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);

    }

    public void Quit(){
        Application.Quit();
    }
    //go to specific scene

     
}
    
