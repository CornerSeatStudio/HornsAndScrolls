using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class SaveData : MonoBehaviour
{
    public int level;
    public int health;
    public float[] position;

    public float Scene;
    public SaveData(){
        scene=SceneManager.GetActiveScene;

    }
    

}
