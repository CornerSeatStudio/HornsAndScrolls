using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] private Rigidbody playerrb; //serializefield allows a private field to be seen in unity editor
    private Vector3 inputVector; 
    public float movementSpeed = 10f; 
    public float Fgravity = 9.81f;
    public float jumpHeight = 1f;
    //to declare mouse vs key presedence, compare via > operator
    public float mouseRotationSmoothness = 7f;
    public float keyRotationSmoothness = 25f;
    [SerializeField] private Animator animator;

    void Start() {
        playerrb = this.GetComponent<Rigidbody>(); //a component is each thing in the unity inspector section, this just pulls it from there
        animator = this.GetComponent<Animator>();
        //UnityEngine.Debug.Log("Hello World");
        //NEVER APPLY ROOT MOTION
        animator.applyRootMotion = false;
        //NEVER FLOP
        playerrb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update() {
        //Player Movement - normalized makes diagonal movement same as normal movement
        //https://answers.unity.com/questions/1370941/more-advanced-player-movement.html
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        inputVector*=movementSpeed;

        //mouse movement - check later if this should be in fixed update instead
        //test - only face player to mouse when aiming
        if(Input.GetButton("Fire1")) {
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
            if (inputVector != Vector3.zero){
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputVector), keyRotationSmoothness * Time.deltaTime);
            }
        }

        
        //anim stuff
        animator.SetBool("IsWalking", inputVector[0] != 0 || inputVector[2] != 0);
        
    }

    void FixedUpdate() {

        playerrb.MovePosition(transform.position + inputVector * Time.deltaTime);

        //el graviolo
        playerrb.AddForce(-transform.up * Fgravity, ForceMode.Acceleration);
        
        //el jump-o - todo: fix so only jumps once rather than act like some dumbass jetpack
        // if(Input.GetAxisRaw("Jump") != 0){
        //     playerrb.AddForce(transform.up * Mathf.Sqrt(2*Fgravity*jumpHeight), ForceMode.VelocityChange);
        // }
        
    }


}