using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public float camMoveMagnitude;
    public float camMoveSmoothness;
    public float mDelta;

    public Vector2 xBounds;
    public Vector2 zBounds;

    private Camera cam;
    private Vector2 input;
    private Vector3 moveVel, increaseFromPosition;
    private bool inBounds = true;

    public void Start(){
        this.GetComponent<Camera>();

    }

    

    public void Update(){
        MoveMapMouseManager();
        

    }

    private void MoveMapMouseManager(){
        //input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        increaseFromPosition = Vector3.zero;

        if(Input.mousePosition.x >= Screen.width - mDelta){
            increaseFromPosition.x += Time.deltaTime;
        }

        if(Input.mousePosition.x <= 0 + mDelta){
            increaseFromPosition.x -=  Time.deltaTime;
        }

        if(Input.mousePosition.y >= Screen.height - mDelta){
            increaseFromPosition.z +=  Time.deltaTime;

        }

        if(Input.mousePosition.y <= 0 + mDelta) {
            increaseFromPosition.z -=  Time.deltaTime;

        }

       increaseFromPosition.Normalize();
       increaseFromPosition*=camMoveMagnitude;
    }

    Vector3 truePosChange; //-210, 460, z:-200, 190
    public void FixedUpdate(){
        //camera move stuff
        truePosChange = Vector3.SmoothDamp(truePosChange, increaseFromPosition, ref moveVel, camMoveSmoothness);
        float xChange = Mathf.Clamp(transform.position.x + truePosChange.x, xBounds.x, xBounds.y);
        float zChange = Mathf.Clamp(transform.position.z + truePosChange.z, -zBounds.x, zBounds.y);
        Vector3 trueMove = new Vector3(xChange, transform.position.y, zChange);
        transform.position = Vector3.Lerp(transform.position, trueMove, Time.fixedDeltaTime);        

    }


}
