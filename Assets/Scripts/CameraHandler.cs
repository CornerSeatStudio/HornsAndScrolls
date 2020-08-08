using UnityEngine;

public class CameraHandler : MonoBehaviour {

    public Transform target; //the player
    [Range(0.01f, 10.0f)] public float cameraSnapSmoothing = 2.5f; //camera snapping to player
    [Range(0.01f, 5.0f)] public float scrollZoomSmoothing = 4f;
    [Range(0.01f, 2.0f)] public float rotationSmoothness = .2f;

    public float camRotateSpeed = 500f;

    public float scrollZoomSpeed = 5f;

    public float maxDist = 80f;
    public float minDist = 20f;

    public float camDist = 40f;
    public float camAng = 30f;

    public float camDisp = -10f;

    private Camera cam; 
    private float scrollInput;
    private Vector3 offset;
    private Vector3 moveVel;
    private float distVel;
    private float angVel;

    private float angle;
    private float tempAddAng; //for the transition between one angle to the next per x frames


    void Start() {
        try {
            target = FindObjectOfType<PlayerHandler>().transform; 
        } catch {
            Debug.LogWarning("WHERE THE PLAYER AT FOOL");
        }
        cam = this.GetComponent<Camera>();
    }

    //general rule: read data in update, handle data in fixed update
    //more here: https://www.youtube.com/watch?v=MfIsp28TYAQ
    void FixedUpdate() { 
        //move camera relative to player
        transform.position = Vector3.SmoothDamp(transform.position, target.position + OffsetAngleCalc(camDisp) + offset, ref moveVel, cameraSnapSmoothing * Time.fixedDeltaTime);

        //camera zoom
        camDist = Mathf.Clamp(Mathf.SmoothDamp(camDist, (scrollInput * scrollZoomSpeed) + camDist, ref distVel, scrollZoomSmoothing * Time.deltaTime), minDist, maxDist);
    
        //angle change based on q/e
        angle = Mathf.SmoothDampAngle(angle, tempAddAng, ref angVel, rotationSmoothness);
        transform.rotation = Quaternion.Euler(camAng, angle, 0);
    }

    private Vector3 DirectionGivenAngle(float angle){
        return new Vector3(-camDist * Mathf.Sin(angle * Mathf.Deg2Rad), camDist, -camDist * Mathf.Cos(angle*Mathf.Deg2Rad));
    }

    private Vector3 OffsetAngleCalc(float offset){
        return new Vector3(offset * Mathf.Sin(angle * Mathf.Deg2Rad), 0, offset * Mathf.Cos(angle*Mathf.Deg2Rad));
    }

    void Update() {
        //scroll input for zoom
        scrollInput = Input.GetAxisRaw("Mouse ScrollWheel") != 0 ? RawCast(Input.GetAxisRaw("Mouse ScrollWheel")) : 0;

        //q/e input for angle
        tempAddAng = angle;
        if (Input.GetKey(KeyCode.Q)) tempAddAng -= Time.deltaTime * camRotateSpeed;
        if (Input.GetKey(KeyCode.E)) tempAddAng += Time.deltaTime * camRotateSpeed;

        //get offset and camera rotation right
        offset = DirectionGivenAngle(angle);
    }

    private float RawCast(float currVal){
        return currVal < 0 ? 1 : -1;
    }

}


