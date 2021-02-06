using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;


//simply displays AI "rings of detection" (implemented somewhere in Behavior tree), along with path finding debug
[CustomEditor (typeof (AIHandler))]
public class AIEditor : Editor
{
    public virtual void OnSceneGUI() {
        //get a reference to the target (member variable of editor which references object in question)
        AIHandler ai = (AIHandler)target;

        //do the magic
        CombatZones(ai);
        PatrolRoutes(ai);
        DrawAIPathing(ai);
    }

    //display bt rings
    private void CombatZones(AIHandler ai) {
        Handles.color = Color.red;
        Handles.DrawWireArc(ai.transform.position, Vector3.up, Vector3.forward, 360, ai.tooFarFromPlayerDistance);
        Handles.DrawWireArc(ai.transform.position, Vector3.up, Vector3.forward, 360, ai.backAwayDistance);
        Handles.DrawWireArc(ai.transform.position, Vector3.up, Vector3.forward, 360, ai.shoveDistance);
    }

    private void DrawAIPathing(AIHandler ai) {
        Handles.color = Color.green;
        List<Vector3> pathCorners = new List<Vector3>();
        if (ai.GetComponent<NavMeshAgent>().hasPath) {
            foreach(Vector3 corner in ai.GetComponent<NavMeshAgent>().path.corners){
                pathCorners.Add(corner);
            }
            Handles.DrawAAPolyLine(pathCorners.ToArray());
        }
    }

    private void PatrolRoutes(AIHandler ai) {
        Handles.color = Color.white;
        List<Vector3> waypoints = new List<Vector3>();
        if(waypoints.Count != 0){
            foreach(PatrolWaypoint p in ai.patrolWaypoints){
                waypoints.Add(p.transform.position);
            }
            Handles.DrawAAPolyLine(waypoints.ToArray());
        }
    }
}
