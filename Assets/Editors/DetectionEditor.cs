using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (Detection))]
public class DetectionEditor : Editor
{        
    public virtual void OnSceneGUI() {
        Detection fov = (Detection)target;
        setup(fov);

    }

    

    public void setup(Detection fov){
        //view arc
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius); //view radius
        Vector3 viewAngleA = fov.DirectionGivenAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.DirectionGivenAngle(fov.viewAngle/2, false);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        //target identification debug
        Handles.color = Color.red;
        foreach(Transform visibleTarget in fov.VisibleTargets){
            Handles.DrawLine(fov.transform.position, visibleTarget.position);
        }
    }
    
}

/*
[CustomEditor (typeof (RaycastAttackHandler))]
public class RaycastAttackHandlerEditor : Editor
{        
    public virtual void OnSceneGUI() {
        RaycastAttackHandler attackHandler = (RaycastAttackHandler)target;
        setupR(attackHandler);

    }

    public void setupR(RaycastAttackHandler attackHandler){
        //view arc
        Handles.color = Color.yellow;
        Handles.DrawWireArc(attackHandler.transform.position, Vector3.up, Vector3.forward, 360, attackHandler.range); //view radius
        Vector3 viewAngleA = attackHandler.DirectionGivenAngle(-attackHandler.angle / 2, false);
        Vector3 viewAngleB = attackHandler.DirectionGivenAngle(attackHandler.angle/2, false);
        Handles.DrawLine(attackHandler.transform.position, attackHandler.transform.position + viewAngleA * attackHandler.range);
        Handles.DrawLine(attackHandler.transform.position, attackHandler.transform.position + viewAngleB * attackHandler.range);

        //target identification debug
        Handles.color = Color.red;
        if(attackHandler.chosenTarget != null) Handles.DrawLine(attackHandler.transform.position, attackHandler.chosenTarget.transform.position);
    }
    
}

*/