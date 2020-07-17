﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : CharacterHandler {

    private Vector3 inputVector;
    private CharacterController controller;
    public bool ToggledWalk {get; private set; } = false;
    private bool weaponDrawn;
    public float CurrMovementSpeed {get; set; }
    
    private Vector3 velocity;
    public float NormalMoveBlend {get; set; }

    [Header("Player Movement Variables")]
    public float jogSpeed;
    public float sprintSpeed;
    public float walkSpeed;
    public float crouchWalkSpeed;
    public float combatMoveSpeed;
    [Range(0, 1)] public float turnSmoothness;

    public float dodgeTime;
    public float dodgeSpeed;
    

    #region callbacks
    protected override void Start(){
        base.Start();
        controller = this.GetComponent<CharacterController>();
        
        SetStateDriver(new IdleMoveState(this, animator)); //player starts unaggro move state

        lastPos = transform.position;

        CurrMovementSpeed = NormalMoveBlend = jogSpeed;
    }


    protected override void Update() {
        //base.Update(); 
        if(genericState != null) debugState.SetText(genericState.ToString());
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        DetermineInputOutcome();
    }

    void FixedUpdate() {
        DealWithGravity();
        if(genericState is DodgeState) {
            controller.Move(dodgeDirection.normalized * dodgeSpeed *  Time.fixedDeltaTime);
        } else {
            controller.Move(inputVector.normalized * CurrMovementSpeed * Time.fixedDeltaTime);  
        }   
    }
    #endregion

    #region chieftan fish
    //master if statement
    private void DetermineInputOutcome() {
        if(genericState is MoveState) { 
            //check for sheathing
            if(Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Fire1")) { 
               // SetStateDriver(new IdleMoveState(this, animator));
                //animator.SetBool(AnimationHashes["Idle"], false);
                animator.SetBool(AnimationHashes["WeaponOut"], true);
                SetStateDriver(new DefaultCombatState(this, animator));
            } else {
                HandleNormalMovement();
                FaceKeyPress();
            }
        } else if (genericState is CombatState) { 
            //check for sheathing
            if(Input.GetKeyDown(KeyCode.X) && !(genericState is DodgeState)) { // but not dodge state
                animator.SetBool(AnimationHashes["WeaponOut"], false); //manually i guess
                SetStateDriver(new IdleMoveState(this, animator));
            } else {
                HandleCombatMovement();
                HandleInteractions();
                FaceMouseDirection();
            }
        }
    }
    #endregion

    #region big fish
    private void HandleNormalMovement() {
        if(genericState is JogMoveState) { //if i am jogging
            if(inputVector.x == 0 && inputVector.z == 0) SetStateDriver(new IdleMoveState(this, animator)); //and stop, stop
            else if(Input.GetKeyDown(KeyCode.LeftShift)) SetStateDriver(new SprintMoveState(this, animator)); //and shift, sprint
            else if(Input.GetKeyDown(KeyCode.CapsLock)) {SetStateDriver(new WalkMoveState(this, animator)); ToggledWalk = true;} //and toggle caps, walk
            else if(Input.GetKeyDown(KeyCode.C)) SetStateDriver(new CrouchWalkMoveState(this, animator)); //and toggle C, crouch walk
        } else if (genericState is SprintMoveState) { //if i am sprinting
            if(inputVector.x == 0 && inputVector.z == 0) SetStateDriver(new IdleMoveState(this, animator)); //and stop, stop
            else if(Input.GetKeyUp(KeyCode.LeftShift)) SetStateDriver(new JogMoveState(this, animator)); //and release shift, jog
            else if(Input.GetKeyDown(KeyCode.C)) SetStateDriver(new CrouchWalkMoveState(this, animator)); //and press c, crouch walk
        } else if (genericState is WalkMoveState) { //if i am walking
            if(inputVector.x == 0 && inputVector.z == 0) SetStateDriver(new IdleMoveState(this, animator)); //and stop, stop
            else if(Input.GetKeyDown(KeyCode.CapsLock)) {SetStateDriver(new JogMoveState(this, animator)); ToggledWalk = false;} //and toggle caps, walk
            else if(Input.GetKeyDown(KeyCode.C)) SetStateDriver(new CrouchWalkMoveState(this, animator)); //and toggle c, crouch walk
        } else if (genericState is CrouchIdleMoveState) { //if i am crouched and still
            if(inputVector.x != 0 || inputVector.z != 0) SetStateDriver(new CrouchWalkMoveState(this, animator)); //and start moving, crouch walk
            else if(Input.GetKeyDown(KeyCode.C)) SetStateDriver(new IdleMoveState(this, animator));   //and toggle c, stand up
        } else if (genericState is CrouchWalkMoveState) { //if i am crouch walking
            if(inputVector.x == 0 && inputVector.z == 0) SetStateDriver(new CrouchIdleMoveState(this, animator)); //and stop, crouch idle
            else if(Input.GetKeyDown(KeyCode.C)) SetStateDriver(new JogMoveState(this, animator)); //and toggle c, jog normally
        } else { //if i am idle
            if(inputVector.x != 0 || inputVector.z != 0){ //and i start walking
                if(!ToggledWalk) { SetStateDriver(new JogMoveState(this, animator)); } //and am in jog mode, jog
                else { SetStateDriver(new WalkMoveState(this, animator)); } //and am in walk mode, walk
            } else if(Input.GetKeyDown(KeyCode.C)) SetStateDriver(new CrouchIdleMoveState(this, animator));   //and toggle c, crouch idle
              else if(Input.GetKeyDown(KeyCode.CapsLock)) ToggledWalk = !ToggledWalk;
        }

    }
    
    private Vector3 dodgeDirection;
    private void HandleCombatMovement() {
        //all feet stuff handled here, not in state
        //determine if smoovin
        //do homemade velocity calculation cause shit be buggy
        CalculateVelocity();

        //if not dodging
        if(genericState is DodgeState){
            Vector3 localDir = transform.InverseTransformDirection(dodgeDirection).normalized;
            animator.SetFloat(AnimationHashes["XCombatMove"], localDir.x);
            animator.SetFloat(AnimationHashes["ZCombatMove"], localDir.z);
        } else {
            if(Input.GetButtonDown("Jump") && (inputVector.x != 0 || inputVector.z != 0)) {
                dodgeDirection = velocity;
                SetStateDriver(new DodgeState(this, animator, dodgeDirection));                
            } 

            Vector3 localDir = transform.InverseTransformDirection(velocity).normalized;
            animator.SetFloat(AnimationHashes["XCombatMove"], localDir.x);
            animator.SetFloat(AnimationHashes["ZCombatMove"], localDir.z);
            animator.SetBool(AnimationHashes["CombatWalking"], (inputVector.x != 0f) || (inputVector.z != 0f));
        }

        
    }

    private void HandleInteractions() {
        if(genericState is DefaultCombatState) {
            if(Input.GetButtonDown("Fire1") == true) {
                Debug.Log("Fire1'd");
                SetStateDriver(new AttackState(this, animator));
            } else if(Input.GetButtonDown("Fire2") == true) {
                Debug.Log("Fire2'd/counter trigger");
                SetStateDriver(new CounterState(this, animator));
                //counter event is here
            }
     
        } else if (genericState is BlockState) { //gets to here after "counter timer" runs up, aka has to wait for a bit to release block
            //if still blocking
            if(Input.GetButton("Fire2") == true) {
                Debug.Log("holding blocking");
            } else {
                Debug.Log("release block");
                SetStateDriver(new DefaultCombatState(this, animator));
            }
        }
    }

    #endregion

    #region small fish
    Vector3 oldPos;
    Vector3 movedPos;
    Vector3 lastPos; //math stuff
    private void CalculateVelocity(){
        oldPos = movedPos;
        movedPos = Vector3.Slerp(oldPos, transform.position - lastPos, .2f); 
        lastPos = transform.position; 
        velocity = movedPos / Time.fixedTime;      
    }

    private void DealWithGravity() {
        if(controller.isGrounded){ 
            inputVector.y = 0;
        } else {
            inputVector.y-=10;
        }
    }

    private void FaceKeyPress() {
        if (inputVector.x != 0 || inputVector.z != 0){ //independent of y
            //Debug.Log("turnign");
            Quaternion faceDirection = Quaternion.LookRotation(inputVector);
            faceDirection.x = 0;
            faceDirection.z = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, faceDirection, turnSmoothness);
        } 
    }

    private void FaceMouseDirection() {
        Plane playerPlane = new Plane(Vector3.up, transform.position);
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        float hitDistance = 0.0f;

        if(playerPlane.Raycast(ray, out hitDistance)) {
            Vector3 targetPoint = ray.GetPoint(hitDistance);
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);

            //dont rotate x or z axis
            targetRotation.x = 0;
            targetRotation.z = 0;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, .2f);
        }    
    }

    #endregion

}
