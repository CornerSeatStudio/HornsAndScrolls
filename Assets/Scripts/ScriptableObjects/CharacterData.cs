using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData")]
public class CharacterData : ScriptableObject {
    //description
    public float maxHealth;
    public float maxStamina; 
    [Range(0, 1)] public float blockQuota;

    public float staminaRegenerationRatePerS;
    //list of interactables

    public float speed;
    public float rel_gravity;
    
    //inventory


}