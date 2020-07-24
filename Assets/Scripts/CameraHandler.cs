using UnityEngine;

public class CameraHandler : MonoBehaviour {
    public Transform target; //the player
    [Range(0.01f, 10.0f)] public float cameraSnapSmoothing = 2.5f; //camera snapping to player
    [Range(0.01f, 5.0f)] public float scrollZoomSmoothing = 4f;
    public float scrollZoomSpeed = 5f;


    public Vector3 maxOffset;
    public Vector3 minOffset;

    public float dist;

 
    private Camera cam; 
    private float scrollInput;
    public Vector3 offset;
    
    private Vector3 moveVel;
    private Vector3 offsetVel;

    void Start(){
        cam = this.GetComponent<Camera>();
        offset = new Vector3(0, 30f, -25f);
        

    }

    //general rule: read data in update, handle data in fixed update
    //more here: https://www.youtube.com/watch?v=MfIsp28TYAQ
    void FixedUpdate() { 
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref moveVel, cameraSnapSmoothing * Time.fixedDeltaTime);
        transform.position = smoothedPosition;

        
        // Debug.Log("SCROLLING DOWN: " + (scrollInput < 0));
        // Debug.Log("SCROLLING UP: " + (scrollInput > 0));
        
    }

    void Update() {
        //scroll for camera movement (within bounds)
        // //probably should deal with jitter - possibly use a lerp
        scrollInput = Input.GetAxisRaw("Mouse ScrollWheel") != 0 ? RawCast(Input.GetAxisRaw("Mouse ScrollWheel")) : 0;
      //  Debug.Log(scrollInput);






        //if WITHIN BOUNDS
        if(offset.sqrMagnitude >= minOffset.sqrMagnitude && offset.sqrMagnitude <= maxOffset.sqrMagnitude
            || offset.sqrMagnitude <= minOffset.sqrMagnitude && scrollInput < 0
            || offset.sqrMagnitude >= maxOffset.sqrMagnitude && scrollInput > 0) {
            
            float offsetDist = scrollInput * scrollZoomSpeed;
            if(offsetDist != 0){
                Vector3 newPos = new Vector3(0, offset.y - offsetDist, offset.z + offsetDist);
                Vector3 smoothPos = Vector3.SmoothDamp(offset, newPos, ref offsetVel, scrollZoomSmoothing * Time.fixedDeltaTime);
                offset = smoothPos;
            }

        } 

    }


    private float RawCast(float currVal){
        return currVal > 0 ? 1 : -1;
    }

}
