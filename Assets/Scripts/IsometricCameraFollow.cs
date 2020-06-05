using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour {
    public Transform target; //the player
    [Range(0.01f, 1.0f)] public float smoothSpeed = 0.125f; //camera snapping to player
    public Vector3 offset; //defaults are {0,7,-10} - how far camera is from player
    public float scrollZoomMultiplier = 5f;
    public float sizeUpperBound = 23f;
    public float sizeLowerBound = 6f;
    [SerializeField] private Camera cam; 

    void Start(){
        cam = this.GetComponent<Camera>();
    }

    //general rule: read data in update, handle data in fixed update
    //more here: https://www.youtube.com/watch?v=MfIsp28TYAQ
    void FixedUpdate() { 
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void Update() {
        //scroll for camera movement (within bounds)
        //probably should deal with jitter - possibly use a lerp
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if(cam.orthographicSize >= sizeLowerBound && cam.orthographicSize <= sizeUpperBound){
            cam.orthographicSize-=scrollInput * scrollZoomMultiplier;
        } else if (cam.orthographicSize < sizeLowerBound) {
            cam.orthographicSize = sizeLowerBound;
        } else if (cam.orthographicSize > sizeUpperBound) {
            cam.orthographicSize = sizeUpperBound;
        }

    }

}
