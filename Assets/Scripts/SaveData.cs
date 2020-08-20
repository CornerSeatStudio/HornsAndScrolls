using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[System.Serializable]
public class SaveData : MonoBehaviour
{
    public static SaveData current;

    public int level;
    public int health;
    public float[] position;

    public float scene;
    public SaveData(){
        scene=SceneManager.GetActiveScene().buildIndex;

    }
    

}
