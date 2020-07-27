using UnityEngine;

public class CameraHandler : MonoBehaviour {

    public Transform target; //the player
    [Range(0.01f, 10.0f)] public float cameraSnapSmoothing = 2.5f; //camera snapping to player
    [Range(0.01f, 5.0f)] public float scrollZoomSmoothing = 4f;
    public float rotationSmoothness = 4f;

    public float camRotateSpeed;

    public float scrollZoomSpeed = 5f;

    public float maxDist;
    public float minDist;

    public float camDist;
    public float camAng;


    private Camera cam; 
    private float scrollInput;
    private Vector3 offset;
    private Vector3 moveVel;
    private float distVel;
    private float angVel;

    private float angle;


    void Start(){
        cam = this.GetComponent<Camera>();
    }

    //general rule: read data in update, handle data in fixed update
    //more here: https://www.youtube.com/watch?v=MfIsp28TYAQ
    void FixedUpdate() { 
        //move camera relative to player
        transform.position = Vector3.SmoothDamp(transform.position, target.position + offset, ref moveVel, cameraSnapSmoothing * Time.fixedDeltaTime);

        //camera zoom
        camDist = Mathf.Clamp(Mathf.SmoothDamp(camDist, (scrollInput * scrollZoomSpeed) + camDist, ref distVel, scrollZoomSmoothing * Time.deltaTime), minDist, maxDist);
    }

    private Vector3 DirectionGivenAngle(float angle){
        return new Vector3(camDist * Mathf.Sin(angle * Mathf.Deg2Rad), camDist, camDist * Mathf.Cos(angle*Mathf.Deg2Rad));
    }

    void Update() {
        scrollInput = Input.GetAxisRaw("Mouse ScrollWheel") != 0 ? RawCast(Input.GetAxisRaw("Mouse ScrollWheel")) : 0;

        float tempAddAng = angle;
        if (Input.GetKey(KeyCode.Q)) tempAddAng -= Time.deltaTime * camRotateSpeed;
        if (Input.GetKey(KeyCode.E)) tempAddAng += Time.deltaTime * camRotateSpeed;

        //get offset and camera rotation right
        angle = Mathf.SmoothDampAngle(angle, tempAddAng, ref angVel, rotationSmoothness);
        offset = DirectionGivenAngle(angle);
        transform.rotation = Quaternion.Euler(camAng, angle-180, 0);

    }

    private float RawCast(float currVal){
        return currVal < 0 ? 1 : -1;
    }

}


