﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    private CharacterController controller;
    private Animator animator;

    private bool IsSprinting;
    private bool IsSlowWalking;
    private float directionofdodgeFB;
    private float directionofdodgeLR;
    private bool IsCrouching=false;
    private bool IsWalking;
    private bool IsDodging;
    Vector3 angleOfPlayer;
    private bool IsWalkingBack;
    private bool IsWalkingRight;
    private bool IsCrouchingWalking;
    private bool IsDodgingRight;
    private bool IsDodgingLeft;
    private bool IsDodgingBack;
    private float rachedtimer;
    private Vector3 inputVector; //key inputs
    private Vector3 dodgingdirections; //dodging stuff
    private bool IsWeaponout;
    public float movementSpeed = 15f; //5 is current goldilocks 
    public float fallspeed = 10f;
    //to declare mouse vs key presedence, compare via > operator
    public float mouseRotationSmoothness = 7f;
    private bool IsWalkingLeft;
    public float keyRotationSmoothness = 15f;
    
    void Start() {
        animator = this.GetComponent<Animator>();
        controller = this.GetComponent<CharacterController>();
//        Debug.Log("Hello World");
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
            //Debug.Log(inputVector);
        }

        //Crouching if statement
        if(Input.GetKeyDown(KeyCode.C)&&!IsCrouching&&(inputVector.x == 0 || inputVector.z == 0)&&!IsSprinting){
            IsCrouching=true;
        }else if(Input.GetKeyDown(KeyCode.C)&&IsCrouching){
            IsCrouching=false;
        }

        //Crouch walking if statement
        if((inputVector.x != 0 || inputVector.z != 0)&&!IsSprinting&&IsCrouching){
            IsCrouchingWalking = true;
            movementSpeed = 10f;
            IsWeaponout=false;

        }else if(inputVector.x ==0 && inputVector.z == 0){
            IsCrouchingWalking=false;
        }

        //sprinting if statements
        if(Input.GetKeyDown(KeyCode.LeftShift)&&!IsCrouching&&!IsCrouchingWalking){
            movementSpeed=25f;
            IsSprinting=true;
            IsWeaponout=false;
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
        
        //sheathe weapon
        if(Input.GetKeyDown(KeyCode.X)&&!IsWeaponout&&!IsSprinting&&!IsCrouching){

            IsWeaponout=true;
                
        }else if((Input.GetKeyDown(KeyCode.X)&&IsWeaponout)||IsSprinting||IsCrouching){
            IsWeaponout=false;
             //independent of y
        }




        //walking at different angles
        angleOfPlayer=transform.eulerAngles;
        if(IsWeaponout&&((210>angleOfPlayer.y&&angleOfPlayer.y>130&&inputVector.z>0))||((330<angleOfPlayer.y||angleOfPlayer.y<30)&&inputVector.z<0)||(60<angleOfPlayer.y&&angleOfPlayer.y<120&&inputVector.x<0)||(240<angleOfPlayer.y&&angleOfPlayer.y<300&&inputVector.x>0)){
            IsWalkingBack=true;
         }
        else {
            IsWalkingBack=false;
        }
        if((210>angleOfPlayer.y&&angleOfPlayer.y>130&&inputVector.x<0)||((330<angleOfPlayer.y||angleOfPlayer.y<30)&&inputVector.x>0)||(60<angleOfPlayer.y&&angleOfPlayer.y<120&&inputVector.z<0)||(240<angleOfPlayer.y&&angleOfPlayer.y<300&&inputVector.z>0)){
             IsWalkingRight=true;
         }
        else {
             IsWalkingRight=false;
        }
        if((210>angleOfPlayer.y&&angleOfPlayer.y>130&&inputVector.x>0)||((330<angleOfPlayer.y||angleOfPlayer.y<30)&&inputVector.x<0)||(60<angleOfPlayer.y&&angleOfPlayer.y<120&&inputVector.z>0)||(240<angleOfPlayer.y&&angleOfPlayer.y<300&&inputVector.z<0)){
            IsWalkingLeft=true;

        }else{
            IsWalkingLeft=false;
        }


        //Dodging

        if(IsWeaponout&&Input.GetKeyDown(KeyCode.Space)){
            directionofdodgeFB=inputVector.z;
            directionofdodgeLR=inputVector.x;
            if(directionofdodgeFB>0){
                IsDodging=true;
            }else if(directionofdodgeFB<0){
                IsDodgingBack=true;
            }
            if(directionofdodgeLR>0){
                IsDodgingRight=true;
            }else if(directionofdodgeLR<0){
                IsDodgingLeft=true;
            }
            rachedtimer=30f;
        }else if(rachedtimer==0&&IsDodging){
            IsDodging=false;
        }
        rachedtimer=rachedtimer-1;
        dodgingdirections = new Vector3(3f*directionofdodgeLR, 0f, 3f*directionofdodgeFB);
        //normal walking if statements
        if((inputVector.x != 0 || inputVector.z != 0)&&!IsCrouching&&!IsSprinting&&!IsSlowWalking&&!IsCrouchingWalking){
            IsWalking=true;
            movementSpeed=15f;
        } else {
            IsWalking=false;
        }
        
        //mouse movement
         if (IsWeaponout) {
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
        if(IsDodging){
            controller.Move(dodgingdirections * Time.fixedDeltaTime);
        }
        if(!IsDodging){
            controller.Move(inputVector * Time.fixedDeltaTime); 
        }

    }

    void LateUpdate(){

        //anim stuff
        animator.SetBool("IsWalking", IsWalking);
        animator.SetBool("IsSprinting", IsSprinting);
        animator.SetBool("IsSlowWalking", IsSlowWalking);
        animator.SetBool("IsCrouching", IsCrouching);
        animator.SetBool("IsCrouchingWalking", IsCrouchingWalking);
        animator.SetBool("IsWalkingBack", IsWalkingBack);
        animator.SetBool("IsWeaponout", IsWeaponout);
        animator.SetBool("IsWalkingRight", IsWalkingRight);
        animator.SetBool("IsWalkingLeft", IsWalkingLeft);
        animator.SetBool("IsDodging", IsDodging);
        animator.SetBool("IsDodgingBack", IsDodgingBack);
        animator.SetBool("IsDodgingRight", IsDodgingRight);
        animator.SetBool("IsDodingLeft", IsDodgingLeft);
    }
    
}