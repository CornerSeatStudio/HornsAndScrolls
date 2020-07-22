using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : CharacterHandler {

    private Vector3 inputVector;
    private CharacterController controller;
    public bool ToggledWalk {get; private set; } = false;
    private bool weaponDrawn;
    public float CurrMovementSpeed {get; set; }
    

    [Header("Player Movement Variables")]
    public float jogSpeed;
    public float sprintSpeed;
    public float walkSpeed;
    public float crouchWalkSpeed;
    public float combatMoveSpeed;
    [Range(0, 1)] public float turnSmoothness;

    public float dodgeTime;
    public float dodgeSpeed;

    [Header("Weapon animation stuff")]
    public GameObject weaponMesh;
    public Transform sheatheTransform;
    public Transform unsheatheTransform;

    #region callbacks
    protected override void Start(){
        base.Start();
        controller = this.GetComponent<CharacterController>();
        
        SetStateDriver(new IdleMoveState(this, animator)); //player starts unaggro move state

        lastPos = transform.position;

        CurrMovementSpeed = jogSpeed;

        
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
        } else if (genericState is AttackState) {
           // controller.Move(transform.forward * 2f * Time.fixedDeltaTime);  //todo how much you move during an attack
        }   else {
            controller.Move(inputVector.normalized * CurrMovementSpeed * Time.fixedDeltaTime);  
        }
    }
/*

    private void OnAnimatorIK(int layerIndex) {
        
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("IKLeftFootWeight"));
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat("IKLeftFootWeight"));       
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, animator.GetFloat("IKRightFootWeight"));
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat("IKRightFootWeight"));
        RaycastHit hit;


        Ray ray = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out hit, distanceToGround + 1f, ~this.gameObject.layer)) {
            if((floor | (1 << hit.transform.gameObject.layer)) == floor) {
                Vector3 footPosition = hit.point;
                footPosition.y += distanceToGround;
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
        } 

        ray = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
        if (Physics.Raycast(ray, out hit, distanceToGround + 1f, ~this.gameObject.layer)) {
            if((floor | (1 << hit.transform.gameObject.layer)) == floor) {
                Vector3 footPosition = hit.point;
                footPosition.y += distanceToGround;
                animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }
        } 
    }


*/
    #endregion

    #region chieftan fish
    //master if statement
    private void DetermineInputOutcome() {
        if(genericState is MoveState) { 
            //check for sheathing
            if(Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Fire1")) { 
                SetStateDriver(new UnsheathingCombatState(this, animator));
            } else {
                HandleNormalMovement();
                FaceKeyPress();
            }
        } else if (genericState is CombatState) { 
            //check for sheathing
            if(Input.GetKeyDown(KeyCode.X) && !(genericState is DodgeState)) { // but not dodge state
                if(genericState is SheathingCombatState) {
                    SetStateDriver(new UnsheathingCombatState(this, animator));
                } else{
                    SetStateDriver(new SheathingCombatState(this, animator));
                }
            } else {
                HandleCombatMovement();
                HandleInteractions();
                if(!(genericState is AttackState)) FaceMouseDirection();
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
        Vector3 localDir;
        
        if(genericState is DodgeState){
            localDir = transform.InverseTransformDirection(dodgeDirection).normalized;
        } else {
            if(Input.GetButtonDown("Jump") && (inputVector.x != 0 || inputVector.z != 0)) {
                dodgeDirection = velocity;
                SetStateDriver(new DodgeState(this, animator, dodgeDirection));                
            } 

            localDir = transform.InverseTransformDirection(velocity).normalized;
            animator.SetBool(Animator.StringToHash("CombatWalking"), (inputVector.x != 0f) || (inputVector.z != 0f));
        }

        animator.SetFloat(Animator.StringToHash("XCombatMove"), localDir.x);
        animator.SetFloat(Animator.StringToHash("ZCombatMove"), localDir.z);
    }

    private void HandleInteractions() {
        if(genericState is DefaultCombatState) {
            if(Input.GetButtonDown("Fire1") == true) {
               // Debug.Log("Fire1'd");
                SetStateDriver(new AttackState(this, animator));
            } else if(Input.GetButtonDown("Fire2") == true) {
                //Debug.Log("Fire2'd/counter trigger");
                SetStateDriver(new CounterState(this, animator));
                //counter event is here
            }
     
        } else if (genericState is BlockState) { //gets to here after "counter timer" runs up, aka has to wait for a bit to release block
            //if still blocking
            if(Input.GetButton("Fire2") == true) {
                //Debug.Log("holding blocking");
            } else {
                //Debug.Log("release block");
                SetStateDriver(new DefaultCombatState(this, animator));
            }
        }
    }

    #endregion

    #region small fish

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

    #region sheathing stuff
   


    public void ParentToSheath() {
        
        weaponMesh.transform.parent = sheatheTransform;
        weaponMesh.transform.localPosition = new Vector3(0.001f, 0.012f,0.0154f);
        weaponMesh.transform.localEulerAngles = new Vector3(315f, 5.2f, 12.6f);
    }

    public void ParentToHand() {
        weaponMesh.transform.parent = unsheatheTransform;
        weaponMesh.transform.localPosition = new Vector3(0.0009f, 0.008f, -0.005f);
        weaponMesh.transform.localEulerAngles = new Vector3(198f, -4.5f, 23.1f);
    }
    #endregion
}
