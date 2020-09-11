using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData 
{
    public int highestLevelIndex;

    public SaveData(SaveManager player){ //upon construction, store the current level index
        highestLevelIndex = SaveManager.HighestLevelIndex;
        Debug.Log($"saved with index {highestLevelIndex}");
    }


}
