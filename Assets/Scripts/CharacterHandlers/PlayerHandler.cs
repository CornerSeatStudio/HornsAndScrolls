﻿using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHandler : CharacterHandler {

    private Vector3 inputVector;
    private CharacterController controller;
    public bool ToggledWalk {get; private set; } = false;
    public float CurrMovementSpeed {get; set; }


    [Header("Player Movement Variables")]
    [Range(0, 1)] public float turnSmoothness;
    public float slopeForceRayLength;
    public float slopeForce;

    [Header("Weapon animation stuff")]
    public GameObject weaponMesh;
    public Transform sheatheTransform;
    public Transform unsheatheTransform;

    public delegate void PickupHandler();
    public event PickupHandler OnInteract;

    public event Action<float> OnStanceChange; 

    #region callbacks
    protected override void Start(){
        base.Start();
        controller = this.GetComponent<CharacterController>();
        
        SetStateDriver(new IdleMoveState(this, animator)); //player starts unaggro move state

        CurrMovementSpeed = (characterdata as PlayerData).jogSpeed;

        if(gameObject.layer != LayerMask.NameToLayer("Player")) Debug.LogWarning ("layer should be set to Player, not " + LayerMask.LayerToName(gameObject.layer));

      //  preVelocity = 0f;
        
    }

    


    protected override void Update() {
        //base.Update(); 
        if(genericState != null) debugState.SetText(genericState.ToString());

        //core move stuff
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        inputVector = Camera.main.transform.TransformDirection(inputVector);
        inputVector.y = 0f;

        DetermineInputOutcome();
    }

    void FixedUpdate() {
        //fix directions -> all speed dependencies occur after this
        inputVector.Normalize();

        if(genericState is DodgeState) {
            controller.SimpleMove(dodgeDirection.normalized * (characterdata as PlayerData).dodgeSpeed );
        } else if (!(genericState is AttackState)) {
            controller.SimpleMove(inputVector * CurrMovementSpeed);  
        }

        if((inputVector.x != 0 || inputVector.z != 0) && OnSlope()) {
            controller.Move(Vector3.down * controller.height / 2 * slopeForce);
        }

    }

    void LateUpdate() {
        CalculateVelocity();
        animator.SetFloat(Animator.StringToHash("PlayerSpeed"), currVelocity.magnitude);
    }

    Vector3 preVelocity, velVel, currVelocity;
    protected void CalculateVelocity(){
        //lerpify velocity
        currVelocity = Vector3.SmoothDamp(preVelocity, controller.velocity, ref velVel, .12f);
        preVelocity = currVelocity;
       // Debug.Log(currVelocity + ", old: " + preVelocity);
    }
    //for slope speed
    private bool OnSlope(){
        RaycastHit hit;
        return Physics.Raycast(transform.position, Vector3.down, out hit, controller.height/2 * slopeForceRayLength) && hit.normal != Vector3.up;
    }

    //for stealth reactoin time
    public void ChangeStanceTimer(float stanceModifier){
        OnStanceChange?.Invoke(stanceModifier);
    }
    

    #endregion

    #region chieftan fish
    //master if statement
    private void DetermineInputOutcome() {
         if(genericState is MoveState) { 
            //check for sheathing
            if(Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Fire1")) { 
                SetStateDriver(new SheathingCombatState(this, animator));
            } else {
                if(!(genericState is SheathingCrouchState)) {
                    HandleNormalMovement();
                    HandleNormalInteractions();
                }
                FaceKeyPress();
            }
        } else {  //combat state (implied already) but not dodge state
            //check for sheathing  
            if(Input.GetKeyDown(KeyCode.X)) {
                if(genericState is SheathingCombatState) {
                    (genericState as SheathingCombatState).ToggleAnim();
                } else {
                    SetStateDriver(new SheathingCombatState(this, animator, true));
                }

            } else if(Input.GetKeyDown(KeyCode.C)) {
                SetStateDriver(new SheathingCrouchState(this, animator));
            } else if (!(genericState is DodgeState)) {
                HandleCombatMovement(); //dodging & direction info
                HandleInteractions(); //clicking
                if(!(genericState is AttackState)) FaceMouseDirection();
            }
        }
    }

    #endregion

    protected override void TakeDamage(float damage, bool isStaggerable, CharacterHandler attacker){
        base.TakeDamage(damage, isStaggerable, attacker);

        if(Health <= 0) { //UPON DEATH
            this.gameObject.SetActive(false);
        }
    }


    #region big fish
    private void HandleNormalInteractions(){
        if(Input.GetKeyDown(KeyCode.F)) {
            OnInteract?.Invoke();
        }
    }

    private void HandleNormalMovement() {
        if(genericState is JogMoveState) { //if i am jogging
            if(inputVector.x == 0 && inputVector.z == 0) SetStateDriver(new IdleMoveState(this, animator)); //and stop, stop
            else if(Input.GetKeyDown(KeyCode.LeftShift) && Stamina > 0) SetStateDriver(new SprintMoveState(this, animator)); //and shift, sprint
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

        //if not dodging
        Vector3 localDir;
        
        if(genericState is DodgeState) { //if i am dodging already, keep dodging in the dodge direction
            localDir = transform.InverseTransformDirection(dodgeDirection).normalized;
        } else { //if i am not dodging
            //get local dir from currVelocity instead
            localDir = transform.InverseTransformDirection(currVelocity).normalized;

            //if i have jumped, do the appropriate setup
            if(Input.GetButtonDown("Jump") && (inputVector.x != 0 || inputVector.z != 0)) {
                dodgeDirection = currVelocity;
                SetStateDriver(new DodgeState(this, animator, dodgeDirection));                
            } 
        }

        //finally set the floats for movement directions
        float weight = Mathf.InverseLerp(0, (characterdata as PlayerData).combatMoveSpeed, currVelocity.magnitude);
        animator.SetFloat(Animator.StringToHash("XMove"), localDir.x * weight);
        animator.SetFloat(Animator.StringToHash("ZMove"), localDir.z * weight);
    
       // Debug.Log(currVelocity);
    }

    private void HandleInteractions() {
        if(genericState is DefaultCombatState) {
            if(Input.GetButtonDown("Fire1") == true) {
               // Debug.Log("Fire1'd");
                SetStateDriver(new AttackState(this, animator));
            } else if(Stamina > 0 && Input.GetButtonDown("Fire2") == true) {
                //Debug.Log("Fire2'd/counter trigger");
                SetStateDriver(new CounterState(this, animator));
                //counter event is here
            }
     
        } else if (genericState is BlockState) { //gets to here after "counter timer" runs up, aka has to wait for a bit to release block
            //if still blocking
            if(!Input.GetButton("Fire2") || Stamina < 0) {
                SetStateDriver(new DefaultCombatState(this, animator));
            } 
        }
    }

    #endregion

    #region small fish

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

    #region sheathing stuff
   


    public void ParentToSheath() {
        
        weaponMesh.transform.parent = sheatheTransform;
        weaponMesh.transform.localPosition = new Vector3(0.001f, 0.02f,0.01f);
        weaponMesh.transform.localEulerAngles = new Vector3(295f, 5.2f, 12.6f);
    }

    public void ParentToHand() {
        weaponMesh.transform.parent = unsheatheTransform;
        weaponMesh.transform.localPosition = new Vector3(0.0009f, 0.008f, -0.005f);
        weaponMesh.transform.localEulerAngles = new Vector3(198f, -4.5f, 23.1f);
    }
    #endregion
}
