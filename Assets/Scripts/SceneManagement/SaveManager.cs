using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class SaveManager : MonoBehaviour
{

    //saving stuff
    public int currentLevelIndex;

    public void Start(){ 
        //upon APP start, load the written file for save data
        //probably doesn't go on start
    }

    public void OnLoad(){
        SaveData data = Saving.Load();

    }

    public void OnSave() {
        Saving.Save(this);
    }

    public void ResetData() {
        currentLevelIndex = 0;
    }

    public void OnLevelCompletion(){
       // Debug.Log("saved?");

        //update completed scene value
        currentLevelIndex = Mathf.Max(currentLevelIndex, SceneManager.GetActiveScene().buildIndex + 1);

       //save it
        Saving.Save(this);
        //move to next scene
        IncrementScene();
    }

    //load most recent scene
    public void OnContinue(){
        SceneManager.LoadScene(currentLevelIndex == 0 ? 1 : currentLevelIndex);
    }

    //start from the beginning (without reseting data)
    public void OnCleanStart(){
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
    
