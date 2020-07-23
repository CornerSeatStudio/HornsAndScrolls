using UnityEngine;

public class CameraHandler : MonoBehaviour {
    public Transform target; //the player
    // [Range(0.01f, 1.0f)] public float cameraSnapSpeed = 0.125f; //camera snapping to player
    // [Range(0.01f, 1.0f)] public float scrollZoomSmoothness = .5f;
    public float scrollZoomSpeed = 5f;
    
    public Vector3 maxOffset;
    public Vector3 minOffset;
    
    private Camera cam; 
    private float scrollInput;
    public Vector3 offset;
    


    void Start(){
        cam = this.GetComponent<Camera>();
        offset = new Vector3(0, 30f, -25f);
    }

    //general rule: read data in update, handle data in fixed update
    //more here: https://www.youtube.com/watch?v=MfIsp28TYAQ
    void FixedUpdate() { 
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosition, .8f);
        transform.position = smoothedPosition;

        
        // Debug.Log("SCROLLING DOWN: " + (scrollInput < 0));
        // Debug.Log("SCROLLING UP: " + (scrollInput > 0));

        //if WITHIN BOUNDS
        if(offset.sqrMagnitude >= minOffset.sqrMagnitude && offset.sqrMagnitude <= maxOffset.sqrMagnitude
            || offset.sqrMagnitude <= minOffset.sqrMagnitude && scrollInput < 0
            || offset.sqrMagnitude >= maxOffset.sqrMagnitude && scrollInput > 0) {
            Vector3 desiredCamPosition = new Vector3(0, offset.y - scrollInput * scrollZoomSpeed, offset.z + scrollInput * scrollZoomSpeed);
            Vector3 smoothedCamPosition = Vector3.Slerp(offset, desiredCamPosition, .8f);
            offset = smoothedCamPosition;
        } 

       // transform.LookAt(new Vector3(target.position.x, target.position.y + 10, target.position.z));
    }

    void Update() {
        //scroll for camera movement (within bounds)
        // //probably should deal with jitter - possibly use a lerp
        scrollInput = Input.GetAxis("Mouse ScrollWheel");
        // if(offset.sqrMagnitude >= minOffset.sqrMagnitude && offset.sqrMagnitude <= maxOffset.sqrMagnitude){
        //     offset.y += scrollInput;
        //     offset.z += scrollInput;
        // } 

    //NEGATIVE IS DOWN, POSITIVE IS UP

        
        


    }

}
