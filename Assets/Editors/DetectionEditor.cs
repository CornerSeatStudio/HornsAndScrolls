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
        Vector3 viewAngleA = fov.directionGivenAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.directionGivenAngle(fov.viewAngle/2, false);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        //interaction arc
        Handles.color = Color.yellow;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, viewAngleA, fov.viewAngle, fov.interactionDistance); //interaction distance


        //target identification debug
        Handles.color = Color.red;
        foreach(Transform visibleTarget in fov.VisibleTargets){
            Handles.DrawLine(fov.transform.position, visibleTarget.position);
        }
    }
    
}

[CustomEditor(typeof(HitDetection))]
public class HitDetectionEditor : DetectionEditor {
    public override void OnSceneGUI(){
        Detection fov = (HitDetection)target;
        base.setup(fov);
        
    }
}
