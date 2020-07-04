using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData")]
public class WeaponData : ScriptableObject
{
    public string WeaponName;
    //public WeaponType weaponType;
    public float startup;
    public float endlag;
    public float damage;
    public float range; //relate to detection script


}
