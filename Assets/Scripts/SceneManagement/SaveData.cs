﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData 
{
    public int currentLevelIndex;

    public SaveData(SaveManager player){
        currentLevelIndex = SaveManager.CurrentLevelIndex;
        Debug.Log($"saved with index {currentLevelIndex}");
    }


}
