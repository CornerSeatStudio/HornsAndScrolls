using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "default", menuName = "ScriptableObjects/MeleeMove")]
public class MeleeMove : ScriptableObject
{
    public float startup;
    public float endlag;
    public float damage;
    public float range; 
    public float angle;
    public bool blockableAttack = true; //special case

}


