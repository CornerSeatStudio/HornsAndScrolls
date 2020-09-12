using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//LEGACY
[CustomEditor (typeof (MeleeRaycastHandler))]
public class MeleeRaycastEditor : Editor
{        
    public virtual void OnSceneGUI() {
        MeleeRaycastHandler zone = (MeleeRaycastHandler)target;
        setup(zone);

    }

    private Vector3 DirectionGivenAngle(float angle){
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle*Mathf.Deg2Rad));
    }

    public void setup(MeleeRaycastHandler zone){
        //view arc
        // Handles.color = Color.yellow;
        // float moveRange = 30f;
        // float moveAngle = 120f; //temp - will be found in logic later
        // Handles.DrawWireArc(zone.transform.position, Vector3.up, Vector3.forward, 360, moveRange); //view radius

        // Vector3 viewAngleA = DirectionGivenAngle(-moveAngle / 2 + zone.transform.eulerAngles.y);
        // Vector3 viewAngleB = DirectionGivenAngle(moveAngle /2 + zone.transform.eulerAngles.y);
        // Handles.DrawLine(zone.transform.position, zone.transform.position + viewAngleA * moveRange);
        // Handles.DrawLine(zone.transform.position, zone.transform.position + viewAngleB * moveRange);

        // //target identification debug
        // Handles.color = Color.red;

        // if(zone.chosenTarget != null)
        // Handles.DrawLine(zone.transform.position, zone.chosenTarget.transform.position);

    }
    
}