using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StaminaPot", menuName = "ScriptableObjects/Consumables/StaminaPot")]
public class StaminaPot : ItemObject {
    public float staminaGain;
    public override void OnUse(CharacterHandler character) {
        character.GainStamina(staminaGain);
    }
}
