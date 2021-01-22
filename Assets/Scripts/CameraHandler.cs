using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class CameraHandler : MonoBehaviour {

    private Transform target; //the player
    [Header("Core Smoothness (higher is longer time")]
    [Range(0.01f, 3)] public float cameraSnapSmoothing = 2.5f; //camera snapping to player

    [Range(0.01f, 3)] public float inputSmoothing = .01f;

    [Header("Scroll wheel stuff")]
   // [Range(0.01f, 3)] public float scrollZoomSmoothing = 4f;
    public float scrollMagnitude = 5f;

    [Header("Default values")]
    public float camDist = 40f;
    public float camAng = 30f;
    public float camDisp = 0f;


    [Header("Distance to player")]
    public Vector2 distRange;

    [Header("FOV stuff")]
    public Vector2 fovRange;

    [Header("AngleOnDistance")]
    public Vector2 angleRange;

    [Header("Cam displacement range")]
    public Vector2 camDispRange;

    [Header("Q/E Rotation")]
    [Range(0.01f, 3)] public float rotationSmoothness = .2f;
    public float camRotateSpeed = 500f;

    [Header("DOF settings")]
    public Vector2 focalDistanceRange;
    public Vector2 apetureRange;
    public Vector2 focalLengthRange;
    


    private Camera cam; 
    private float scrollInput;
    private Vector3 offset;
    private Vector3 moveVel;
    private float distVel, fovVel, angVel, inputVel;
    private float angle;
    private float tempAddAng; //for the transition between one angle to the next per x frames
    private PostProcessVolume postProcessVolume;
    private DepthOfField depthOfField;

    void Start() {
        try {
            target = FindObjectOfType<PlayerHandler>().transform; 
        } catch {
            Debug.LogWarning("WHERE THE PLAYER AT FOOL");
        }
        cam = this.GetComponent<Camera>();

        try { 
        postProcessVolume = GameObject.FindGameObjectWithTag("DepthOfField").GetComponent<PostProcessVolume>();
        if (!postProcessVolume.sharedProfile.TryGetSettings<DepthOfField>(out depthOfField)) 
            Debug.LogWarning("DOF MISSING ON CAM HANDLER");


        } catch { Debug.LogWarning("DOF PROBLEMS"); }

                
             
    }

    //general rule: read data in update, handle data in fixed update
    //more here: https://www.youtube.com/watch?v=MfIsp28TYAQ
    float tempVel;
    void FixedUpdate() { 
        //move camera relative to player
        transform.position = Vector3.SmoothDamp(transform.position, target.position + OffsetAngleCalc(camDisp) + offset, ref moveVel, cameraSnapSmoothing * Time.fixedDeltaTime);

        float distanceDependency = Mathf.InverseLerp(distRange.x, distRange.y, camDist);

        //fov modification - should depend on cam dist
        cam.fieldOfView = Mathf.Lerp(fovRange.y, fovRange.x, 1 - distanceDependency);

        //angle based on zoom
        camAng = Mathf.Lerp(angleRange.x, angleRange.y, distanceDependency);

        //cam disp range
        camDisp = Mathf.Lerp(camDispRange.x, camDispRange.y, distanceDependency);

        //angle change based on q/e
        angle = Mathf.SmoothDampAngle(angle, tempAddAng, ref angVel, rotationSmoothness);
        transform.rotation = Quaternion.Euler(camAng, angle, 0);

        //ppv dof
        try{ 
            if (postProcessVolume.sharedProfile.TryGetSettings<DepthOfField>(out depthOfField)) {
                depthOfField.focusDistance.value = Mathf.Lerp(focalDistanceRange.x, focalDistanceRange.y, distanceDependency);
                depthOfField.aperture.value = Mathf.Lerp(apetureRange.x, apetureRange.y, distanceDependency);
                depthOfField.focalLength.value = Mathf.Lerp(focalLengthRange.x, focalLengthRange.y, distanceDependency);
            }
        } catch { Debug.Log("FIX POST PROCESSING LOL"); }
    }

    
    void Update() {

        float tsi = Input.GetAxis("Mouse ScrollWheel");
        tsi = Mathf.Abs(tsi) > .03f ? tsi < 0 ? 1 : -1 : 0;

        //lerp tsi for smoother shit
        scrollInput = Mathf.SmoothDamp(scrollInput + tsi, 0, ref inputVel, inputSmoothing);


        //camDist = Mathf.Clamp(Mathf.SmoothDamp(camDist, camDist + scrollInput * scrollMagnitude, ref distVel, scrollZoomSmoothing), distRange.x, distRange.y);
        camDist = Mathf.Clamp(camDist + scrollInput * scrollMagnitude, distRange.x, distRange.y);


        //q/e input for angle
        tempAddAng = angle;
        if (Input.GetKey(KeyCode.Q)) tempAddAng -= Time.deltaTime * camRotateSpeed;
        if (Input.GetKey(KeyCode.E)) tempAddAng += Time.deltaTime * camRotateSpeed;

        //get offset and camera rotation right
        offset = DirectionGivenAngle(angle);
    }

    private Vector3 DirectionGivenAngle(float angle){
        return new Vector3(-camDist * Mathf.Sin(angle * Mathf.Deg2Rad), camDist, -camDist * Mathf.Cos(angle*Mathf.Deg2Rad));
    }

    private Vector3 OffsetAngleCalc(float offset){
        return new Vector3(offset * Mathf.Sin(angle * Mathf.Deg2Rad), 0, offset * Mathf.Cos(angle*Mathf.Deg2Rad));
    }

}


