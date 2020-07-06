using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { MELEE, RANGED };

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string WeaponName;
    public List<MeleeMove> MeleeMoves;

}

