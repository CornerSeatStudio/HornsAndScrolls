using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class SaveData : MonoBehaviour
{
   public static SaveData current;
    public string scene;
    public void Start(){
        scene=SceneManager.GetActiveScene().name;
    }
}
    
