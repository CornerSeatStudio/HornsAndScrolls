using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackMove", menuName = "ScriptableObjects/AttackMove")]
public class MeleeAttackMove : ScriptableObject
{
    public float startup;
    public float endlag;
    public float damage;
    public float range; //relate to detection script
    public float angle;

}