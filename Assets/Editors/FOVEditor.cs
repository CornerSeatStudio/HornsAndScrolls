using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (FieldOfView))]
public class FOVEditor : Editor
{        

    public virtual void OnSceneGUI() {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
    
        //two vectors "split" in the middle
        Vector3 viewAngleA = fov.directionGivenAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.directionGivenAngle(fov.viewAngle/2, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        Handles.color = Color.red;
        foreach(Transform visibleTarget in fov.getVisibleTargets()){
            Handles.DrawLine(fov.transform.position, visibleTarget.position);
        }

    }
    
}

[CustomEditor(typeof(AIMechanics))]
public class AIFOVEditor : FOVEditor {
    
    public override void OnSceneGUI(){
        AIMechanics fov = (AIMechanics)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.viewRadius);
    
        //two vectors "split" in the middle
        Vector3 viewAngleA = fov.directionGivenAngle(-fov.viewAngle / 2, false);
        Vector3 viewAngleB = fov.directionGivenAngle(fov.viewAngle/2, false);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.viewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.viewRadius);

        Handles.color = Color.red;
        foreach(Transform visibleTarget in fov.getVisibleTargets()){
            Handles.DrawLine(fov.transform.position, visibleTarget.position);
        }
        
    }
}
