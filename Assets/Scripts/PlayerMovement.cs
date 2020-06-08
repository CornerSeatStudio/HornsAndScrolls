using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] private Rigidbody playerrb; //serializefield allows a private field to be seen in unity editor
    private Vector3 inputVector; 
    public float movementSpeed = 10f; 

    //to declare mouse vs key presedence, compare via > operator
    public float mouseRotationSmoothness = 7f;
    public float keyRotationSmoothness = 25f;
    [SerializeField] private Animator animator;

    void Start() {
        playerrb = this.GetComponent<Rigidbody>(); //a component is each thing in the unity inspector section, this just pulls it from there
        animator = this.GetComponent<Animator>();
        //UnityEngine.Debug.Log("Hello World");
    }

    void Update() {
        //mouse movement
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

        //Player Movement 
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal") * movementSpeed, 0f, Input.GetAxisRaw("Vertical") * movementSpeed);
        playerrb.velocity = inputVector;

        //given i want player movement to take precedent in terms of the direction the player faces, it gets updated last
        //if the player is using the mouse while moving, it will still face run direction?
        //todo: if a player is doign an action (i.e., attacking or interacting idk), prioritize mouse direction
        //make player face walk direction

        if (inputVector != Vector3.zero){
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputVector), keyRotationSmoothness * Time.deltaTime);
        }
        
        //anim stuff
        animator.SetFloat("InputX", inputVector[0]);
        animator.SetFloat("InputZ", inputVector[2]);
        
    }


}