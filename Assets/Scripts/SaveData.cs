using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData : MonoBehaviour
{
    public int level;
    public int health;
    public float[] position;
    public SaveData(PlayerHandler player){
        position = new float[3];
        position[0] = player.sheatheTransform.position.x;
        position[1] = player.sheatheTransform.position.y;
        position[2] = player.sheatheTransform.position.z;
    }
}
