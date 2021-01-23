using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SaveManager : MonoBehaviour
{
    public GameObject levelselectcomponent;
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
        }
    }
    #endregion

    //saving stuff
    public static int HighestLevelIndex {get; private set;} //STATIC LEVEL INDEX - aka the current level


    #region core
    //load the save - only occurs on game start up


    //save the current level index - occurs everytime a level is completed


    //reset current level index - irreversable


    #endregion


    #region scene transitions
    //load most recent scene from main menu (include the case for new game)


    //everytime a normal level is completed, return to camp


    //everytime camp is left, go to level selection
    public void OnLevelSelectLoad(){
        ChooseAndLoadLevel(3);
    }
    public void clickedlevel(){
       levelselectcomponent.SetActive(false);


    }
    public void OnCleanStart() => SceneManager.LoadScene(1); 

    //for manual selection (aka level selection) (mosly use this)
    public void ChooseAndLoadLevel(int buildIndex) => SceneManager.LoadScene(buildIndex);

    //start from the beginning (without reseting data) //assuming 1 is church level
    public void level1() => SceneManager.LoadScene(2); 
    public void level2() => SceneManager.LoadScene(3); 
    #endregion

    #region other
    public void Quit() => Application.Quit();
    

    #endregion
     
}
    
