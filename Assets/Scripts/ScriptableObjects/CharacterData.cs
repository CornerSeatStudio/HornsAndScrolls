using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ScriptableObjects/CharacterData")]
public class CharacterData : ScriptableObject {
    //description
    public float maxHealth;
    public float maxStamina; 

    public float staminaRegenerationRatePerS;
    //list of interactables
    public float staminaRegenerationWindow = 3f;
    //inventory

}


[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : CharacterData {

    public float jogSpeed;
    public float sprintSpeed;
    public float walkSpeed;
    public float crouchWalkSpeed;
    public float combatMoveSpeed;
    public float dodgeTime; //should be length of animation
    public float dodgeSpeed;
    public float detectionTime;
}