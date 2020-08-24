using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HealthPot", menuName = "ScriptableObjects/Consumables/HealthPot")]
public class HealthPot : ItemObject {
    public float healthGain;
    public override void OnUse(CharacterHandler character) {
        character.GainHealth(healthGain);
    }
}

