using UnityEngine;

public class CameraHandler : MonoBehaviour {
    public Transform target; //the player
    [Range(0.01f, 1.0f)] public float cameraSnapSpeed = 0.125f; //camera snapping to player
    [Range(0.01f, 1.0f)] public float scrollZoomSmoothness = .5f;
    
    public Vector3 maxOffset;
    public Vector3 minOffset;
    
    public float scrollZoomMultiplier = 20f;

    private Camera cam; 
    public Vector3 offset;


    void Start(){
        cam = this.GetComponent<Camera>();
        offset = new Vector3(0, 30f, -25f);
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
        // //probably should deal with jitter - possibly use a lerp
        float scrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
        // if(offset.sqrMagnitude >= minOffset.sqrMagnitude && offset.sqrMagnitude <= maxOffset.sqrMagnitude){
        //     offset.y += scrollInput;
        //     offset.z += scrollInput;
        // } 


        if(offset.sqrMagnitude < minOffset.sqrMagnitude){
            offset = minOffset;
        }else if(offset.sqrMagnitude  >= minOffset.sqrMagnitude && offset.sqrMagnitude  <= maxOffset.sqrMagnitude){
            //offset  = Vector3.Lerp(offset, offset  - , scrollZoomSmoothness);
        }
        if(offset.sqrMagnitude  > maxOffset.sqrMagnitude) {
            offset  = maxOffset;
        }else if(offset.sqrMagnitude  >= minOffset.sqrMagnitude && offset.sqrMagnitude  <= maxOffset.sqrMagnitude){
            //offset  = Vector3.Lerp(offset, offset  - , scrollZoomSmoothness);
        }


    }

}
