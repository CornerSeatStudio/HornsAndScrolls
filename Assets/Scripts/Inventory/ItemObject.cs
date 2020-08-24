using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemObject : ScriptableObject {
   // public GameObject prefab; //IN GAME GAMEOBJECT, noT CANVAS ONE
    [TextArea(15, 20)] string description;
    public abstract void OnUse(CharacterHandler character);

}
