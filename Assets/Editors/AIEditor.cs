using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (AIHandler))]
public class AIEditor : Editor
{
    public virtual void OnSceneGUI() {
        AIHandler ai = (AIHandler)target;

        CombatZones(ai);
        PatrolRoutes(ai);
    }

    private void CombatZones(AIHandler ai) {
        Handles.color = Color.red;
        Handles.DrawWireArc(ai.transform.position, Vector3.up, Vector3.forward, 360, ai.tooFarFromPlayerDistance);
        Handles.DrawWireArc(ai.transform.position, Vector3.up, Vector3.forward, 360, ai.backAwayDistance);
        Handles.DrawWireArc(ai.transform.position, Vector3.up, Vector3.forward, 360, ai.shoveDistance);
    }

    private void PatrolRoutes(AIHandler ai) {
        Handles.color = Color.white;
        List<Vector3> waypoints = new List<Vector3>();
        foreach(PatrolWaypoint p in ai.patrolWaypoints){
            waypoints.Add(p.transform.position);
        }
        Handles.DrawAAPolyLine(waypoints.ToArray());
    }
}
