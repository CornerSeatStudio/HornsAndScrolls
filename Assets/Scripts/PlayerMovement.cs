using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private CharacterController controller;
    private Animator animator;

    private bool IsSprinting;
    private bool IsSlowWalking;

    private bool IsWalking;
    private Vector3 inputVector; //key inputs
    public float movementSpeed = 5f; //5 is current goldilocks 
    public float fallspeed = 10f;

    //to declare mouse vs key presedence, compare via > operator
    public float mouseRotationSmoothness = 7f;
    public float keyRotationSmoothness = 15f;

    void Start() {
        animator = this.GetComponent<Animator>();
        controller = this.GetComponent<CharacterController>();
        //NEVER APPLY ROOT MOTION
        animator.applyRootMotion = false;
        Debug.Log("Hello World");
        //temp character contorller settings
        controller.center = new Vector3(0, 2, 0);
        controller.radius = 3;
        controller.height = 15;
    }

    void Update() {
        //Player Movement - normalized makes diagonal movement same as normal movement
        //https://answers.unity.com/questions/1370941/more-advanced-player-movement.html
        
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        
        //no accelaration due to gravity, just a set speed instead
        //will be fixed with math
        inputVector.y-=fallspeed;
        if(controller.isGrounded){ 
            inputVector.y = 0;
        }


        if(Input.GetKeyDown(KeyCode.LeftShift)){
            IsSprinting=true;
            inputVector= new Vector3(10f,0f,10f);
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift)){
            IsSprinting=false;
            inputVector=new Vector3(5f,0f,5f);
        }

        if(Input.GetKeyDown(KeyCode.LeftAlt)){
            IsSlowWalking=true;
            inputVector= new Vector3(2.5f,0f,2.5f);
        }
        else if(Input.GetKeyUp(KeyCode.LeftAlt)){
            IsSlowWalking=false;
            inputVector=new Vector3(5f,0f,5f);
        }
        
        if(inputVector.x != 0 || inputVector.z != 0){
            IsWalking=true;
            inputVector=new Vector3(Input.GetAxisRaw("Horizontal")*5f,0f,Input.GetAxisRaw("Vertical")*5f);
        }


        //mouse movement - check later if this should be in fixed update instead
        //test - only face player to mouse when aiming
        if (Input.GetButton("Fire1")) {
            Plane playerPlane = new Plane(Vector3.up, transform.position);
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
            float hitDistance = 0.0f;

            if(playerPlane.Raycast(ray, out hitDistance)) {
                Vector3 targetPoint = ray.GetPoint(hitDistance);
                Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);

                //dont rotate x or z axis
                targetRotation.x = 0;
                targetRotation.z = 0;

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, mouseRotationSmoothness * Time.deltaTime);

            }
        } else {
            if (inputVector.x != 0 || inputVector.z != 0){ //independent of y
                Quaternion faceDirection = Quaternion.LookRotation(inputVector);
                faceDirection.x = 0;
                faceDirection.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, faceDirection, keyRotationSmoothness * Time.deltaTime);
            } 
        }
    }

    void FixedUpdate() {
        controller.Move(inputVector * Time.fixedDeltaTime); 
    }

    void LateUpdate(){

        //anim stuff
        animator.SetBool("IsWalking", IsWalking);
        animator.SetBool("IsSprinting", IsSprinting);
        animator.SetBool("IsSlowWalking", IsSlowWalking);
    }
    
}