using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectiveItem", menuName = "ScriptableObjects/ObjectiveItem")]
public class ObjectiveObject : ItemObject {
    public Vector3 pickupPosition;
    public Vector3 pickupEulerAngle;
    public Vector3 pickupScale;

    public override void OnUse(CharacterHandler character){

    }

}
