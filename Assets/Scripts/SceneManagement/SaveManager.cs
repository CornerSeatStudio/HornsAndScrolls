using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    #region callbacks
    public void Update(){
        //debug
        // if(Input.GetKeyDown(KeyCode.P)) {
        //     Debug.Log(CurrentLevelIndex);
        // 
    }

    public void Start(){
        //aka if on main menu, load the current save.
        if(SceneManager.GetActiveScene().buildIndex == 0) {
            OnLoad();
        }
    }
    #endregion

    //saving stuff
    public static int HighestLevelIndex {get; private set;} //STATIC LEVEL INDEX - aka the current level


    #region core
    //load the save - only occurs on game start up
    public void OnLoad(){ 
        HighestLevelIndex = Saving.Load().highestLevelIndex;
    }

    //save the current level index - occurs everytime a level is completed
    public void OnSave() {
        Saving.Save(this);
    }

    //reset current level index - irreversable
    public void ResetData() {
        HighestLevelIndex = 0;
        Saving.Save(this);
    }

    #endregion


    #region scene transitions
    //load most recent scene from main menu (include the case for new game)
    public void OnGameStart(){
        //if starting at 0, (aka a new game)
        if(HighestLevelIndex == 0) {
            OnCleanStart();
        } else {
            //load last saved scene
            SceneManager.LoadScene(HighestLevelIndex);
        }
    }

    //everytime a normal level is completed, return to camp
    public void OnStandardLevelCompletion(){
        //update completed scene value with HIGHEST SCENE INDEX
        HighestLevelIndex = Mathf.Max(HighestLevelIndex, SceneManager.GetActiveScene().buildIndex + 1);
        Saving.Save(this);
        ChooseAndLoadLevel(2); //RETURN TO CAMP CAMP (which is currently 2 (subject to change))
    }

    //everytime camp is left, go to level selection
    public void OnLevelSelectLoad(){
        ChooseAndLoadLevel(3);
    }

    //for manual selection (aka level selection) (mosly use this)
    public void ChooseAndLoadLevel(int buildIndex) => SceneManager.LoadScene(buildIndex);

    //start from the beginning (without reseting data) //assuming 1 is church level
    public void OnCleanStart() => SceneManager.LoadScene(1); 
    #endregion

    #region other
    public void Quit() => Application.Quit();

    //goes to next/prev scene(used sparingly, most will be manual)
    // private void IncrementScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    // private void DecrementScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);

    #endregion

     
}
    
