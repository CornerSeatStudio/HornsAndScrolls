using UnityEngine;

public class CameraHandler : MonoBehaviour {
    public Transform target; //the player
    [Range(0.01f, 1.0f)] public float cameraSnapSpeed = 0.125f; //camera snapping to player
    public Vector3 offset = new Vector3(0, 20, -16); //defaults are {0,7,-10} - how far camera is from player
    [Range(0.01f, 1.0f)] public float scrollZoomSmoothness = .5f;
    public float scrollZoomMultiplier = 20f;
    public float fovUpperBound = 120f;
    public float fovLowerBound = 25f;
    public float fov;
    private Camera cam; 

    void Start(){
        cam = this.GetComponent<Camera>();
        //default settings
        cam.fieldOfView = fov;

    }

    //general rule: read data in update, handle data in fixed update
    //more here: https://www.youtube.com/watch?v=MfIsp28TYAQ
    void FixedUpdate() { 
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosition, cameraSnapSpeed);
        transform.position = smoothedPosition;

       // transform.LookAt(new Vector3(target.position.x, target.position.y + 10, target.position.z));
    }

    void Update() {
        //scroll for camera movement (within bounds)
        //probably should deal with jitter - possibly use a lerp
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if(cam.fieldOfView >= fovLowerBound && cam.fieldOfView <= fovUpperBound){
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, cam.fieldOfView - (scrollInput * scrollZoomMultiplier), scrollZoomSmoothness);
        } else if (cam.fieldOfView < fovLowerBound) {
            cam.fieldOfView = fovLowerBound;
        } else if (cam.fieldOfView > fovUpperBound) {
            cam.fieldOfView = fovUpperBound;
        }

    }

}
