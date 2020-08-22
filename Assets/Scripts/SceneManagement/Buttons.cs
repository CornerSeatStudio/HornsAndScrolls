using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Buttons : MonoBehaviour
{
    

    public void NextScene(){
 
    }

    public void PreviousScene(){
    }
    
    public void load(string SceneName){
        SceneManager.LoadScene(SceneName);
    }

}
