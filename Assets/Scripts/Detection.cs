using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    [Range(0, 360)] public float viewAngle = 30;
    public float viewRadius = 10;
    public float interactionDistance = 3;
    public float coroutineDelay = .2f;

    //LayerMasks allow for raycasts to choose what to and not to register
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<Transform> VisibleTargets {get; } = new List<Transform>();
    public List<GameObject> InteractableTargets {get; } = new List<GameObject>();

    [Range(0, 0.25f)] public float meshResolution; //# of triangle divisions of FOV, larger == more circular
    public MeshFilter viewMeshFilter; 
    Mesh viewMesh; 

    //ON DEATH -> STOP COROUTINE

    void Awake() {
        viewMesh = new Mesh(); //has to be called before start
    }
 
    void Start() {
        viewMesh.name = "View Visualization";
        StartCoroutine("FindTargetsWithDelay", coroutineDelay);
        viewMeshFilter.mesh = viewMesh;
     
    } 

    //should be called AFTER dealing with movement
    /**
    void LateUpdate() {
        Debug.Log(viewMeshFilter.mesh + " vs " + viewMesh);
        if (initiatedMesh == null) {
            initiatedMesh = InitiateMesh();
            StartCoroutine(initiatedMesh); 
        }
        DrawFOV(); viewMeshFilter.mesh = viewMesh;
    }
    **/

    //idk why the fuck shit aint workin so
    private IEnumerator initiatedMesh = null;
    private IEnumerator InitiateMesh() {
        yield return new WaitForSeconds(1f);
        viewMeshFilter.mesh = viewMesh;
    }

    private IEnumerator FindTargetsWithDelay(float delay){
        while (true){
            yield return new WaitForSeconds(delay); //only coroutine every delay seconds
            findVisibleTargets();
        }
    }


    //for every target (via an array), lock on em, the core logic
    private void findVisibleTargets(){
        VisibleTargets.Clear(); //reset list every time so no dupes
        InteractableTargets.Clear();
        //cast a sphere over player, store everything inside col
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        foreach(Collider col in targetsInView){
            Transform target = col.transform; //get the targets locatoin
            Vector3 directionToTarget = (target.position - transform.position).normalized; //direction vector of where bloke is
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle/2){ //if the FOV is within bounds, /2 cause left right
                //do the ray 
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)){ //if, from character at given angle and distance, it DOESNT collide with obstacleMask
                    VisibleTargets.Add(target);
                    //Debug.Log("spot a cunt");
                    if(distanceToTarget <= interactionDistance) { //additional check if in range of an interaction
                        InteractableTargets.Add(col.gameObject);
                        //Debug.Log("interactable distance satisfied");
                    }
                }
            }
        }
    }

    //for the actual visualization
    private void DrawFOV() {
        //# rays, where if meshResolution == 1, then there would be one ray per degree
        int rayCount = Mathf.RoundToInt(viewAngle * meshResolution); 
        //angle between each ray
        float rayAngleSize = viewAngle / rayCount; 
        List<Vector3> viewPoints = new List<Vector3>(); //viewpoints change every update loop

        //for every ray
        for (int i = 0; i <= rayCount; ++i) {
            //get the current rotation of the player, starting from the furthest left of the viewAngle
            float angle = transform.eulerAngles.y - (viewAngle/2) + rayAngleSize * i;
            ViewCastInfo viewCast = ConstructViewCast(angle); //for eacy ray, do a struct
            viewPoints.Add(viewCast.endpoint); //and pull endpoint info from said struct (aka triangle vertexes -origin)
        }

        //once info is gathered regarding viewPoints

        int vertexCount = viewPoints.Count + 1; //all triangle verteces
        Vector3[] vertices = new Vector3[vertexCount]; //list of actual verteces
        int[] triangles = new int[(vertexCount - 2) * 3]; //one triangle is 3 verteces
    
        //using a mesh renderer as a CHILD of each character, account for local position
        vertices[0] = Vector3.zero;
        //for every vertex, store them in both arrays as appropriate
        for(int i = 0; i < vertexCount -1; ++i) { 
            vertices[i+1] = transform.InverseTransformPoint(viewPoints[i]); //viewPoints need to become local

            //for every one step in vertices, 3 steps should be taken in triangles
            //also prevent out of bounds stuff
            if(i < vertexCount - 2){
                triangles[i*3] = 0;
                triangles[i*3 + 1] = i+1;
                triangles[i*3 + 2] = i+2;
            }
        }

        viewMesh.Clear(); //reset mesh every loop
        //to build meshes in code, there is need for vertices and triangles
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals(); //good habit i guess
    }

    private ViewCastInfo ConstructViewCast(float globalAngle) {
        Vector3 dirGivenAngle = DirectionGivenAngle(globalAngle, true);
        RaycastHit hit; 

        if(Physics.Raycast(transform.position, dirGivenAngle, out hit, viewRadius, obstacleMask)){
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        } else {
            return new ViewCastInfo(false, transform.position + dirGivenAngle * viewRadius, viewRadius, globalAngle);
        }
    }
    
    //return direction vector given a specific angle
    public Vector3 DirectionGivenAngle(float angle, bool isGlobal){
        if(!isGlobal){
            angle+=transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle*Mathf.Deg2Rad));
    }

    //information regarding a single ray cast
    public struct ViewCastInfo {
        public bool hit; //if the ray hits something
        public Vector3 endpoint; //endpoint of the ray
        public float rayLength; //distance/length of the ray
        public float angle; //angle that ray was fired at

        public ViewCastInfo(bool hit, Vector3 endpoint, float rayLength, float angle) {
            this.hit = hit;
            this.endpoint = endpoint;
            this.rayLength = rayLength;
            this.angle = angle;
        }
    }

}
