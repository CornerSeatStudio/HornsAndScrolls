using UnityEngine;

public class IsometricCameraFollow : MonoBehaviour {
    public Transform target; //the player

    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.125f; //camera snapping to player
    public Vector3 offset; //defaults are {0,7,-10} - how far camera is from player

    //general rule: read data in update, handle data in fixed update
    //more here: https://www.youtube.com/watch?v=MfIsp28TYAQ
    void FixedUpdate() { 
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

}
