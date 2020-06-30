using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private CharacterController controller;
    private Animator animator;

    private bool IsSprinting;
    private bool IsSlowWalking;

    private bool IsCrouching=false;
    private bool IsWalking;
    private bool IsCrouchingWalking;

    private Vector3 inputVector; //key inputs


    public float movementSpeed = 15f; //5 is current goldilocks 
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
        
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal")*movementSpeed, 0f, Input.GetAxisRaw("Vertical")*movementSpeed);

        //no accelaration due to gravity, just a set speed instead
        //will be fixed with math
        inputVector.y-=fallspeed;
        if(controller.isGrounded){ 
            inputVector.y = 0;
        }

        //my nomilization fuck the normal stuff
        if(inputVector.x !=0 && inputVector.z!=0){
            inputVector = new Vector3(Input.GetAxisRaw("Horizontal")*movementSpeed*0.7f, 0f, Input.GetAxisRaw("Vertical")*movementSpeed*0.7f);
        }

        //Crouching if statement
        if(Input.GetKeyDown(KeyCode.C)&&!IsCrouching&&(inputVector.x == 0 || inputVector.z == 0)&&IsSprinting){
            IsCrouching=true;
            movementSpeed = 0f;
        }else if(Input.GetKeyDown(KeyCode.C)&&IsCrouching){
            IsCrouching=false;
        }

        //Crouch walking if statement
        if((inputVector.x != 0 || inputVector.z != 0)&&!IsSprinting){
            IsCrouchingWalking = true;
            movementSpeed = 10f;

        }else{
            IsCrouchingWalking=false;
        }

        //sprinting if statements
        if(Input.GetKeyDown(KeyCode.LeftShift)&&!IsCrouching&&!IsCrouchingWalking){
            movementSpeed=25f;
            IsSprinting=true;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift)){
            IsSprinting=false;
        }


        //slow walking if statements
        if(Input.GetKeyDown(KeyCode.LeftAlt)){
            IsSlowWalking=true;
            movementSpeed=5f;
        }
        else if(Input.GetKeyUp(KeyCode.LeftAlt)){
            IsSlowWalking=false;
        }
        

        //normal walking if statements
        if((inputVector.x != 0 || inputVector.z != 0)&&!IsCrouching&&!IsSprinting&&!IsSlowWalking&&!IsCrouchingWalking){
            IsWalking=true;
            movementSpeed=15f;
        } else{
            IsWalking=false;
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
        animator.SetBool("IsCrouching", IsCrouching);
        animator.SetBool("IsCrouchingWalking", IsCrouchingWalking);
    }
    
}