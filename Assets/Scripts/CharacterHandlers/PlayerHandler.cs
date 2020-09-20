using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;


//all player stuff is handled in a single script
//whether doing it in one class - like here, or splitting it up based on components is a stylistic choice
//i chose to shove it in one because i dont want a gameobject to have like 10 scripts
//this comes with the disadvantage of having like a fuck ton of lines in one script
public class PlayerHandler : CharacterHandler {

    private Vector3 inputVector; //stores input
    private CharacterController controller; //deals with collider and slopes - unity built in thing
    public bool ToggledWalk {get; private set; } = false;
    public float CurrMovementSpeed {get; set; }
    public bool InDialogue {get; set; } = false;


    [Header("Player Movement Variables")]
    [Range(0, 1)] public float turnSmoothness; //how quickly player snaps when pressing wasd
    public float slopeForceRayLength; //for slope velocity handling
    public float slopeForce;

    [Header("Weapon animation stuff")]
    public GameObject weaponMesh;
    public Transform sheatheTransform; 
    public Transform unsheatheTransform;

    [Header("Inventory UI stuff")]
    public TextMeshProUGUI healthPotCount;
    public TextMeshProUGUI staminaPotCount;

    public GameObject StealthRing {get; private set; } //ui thing
    public delegate void PickupHandler();
    public event PickupHandler OnInteract; //invoke an event everytime i interact with something applicable
    public event Action<float> OnStanceChangeTimer;  
    public event Action<float> OnStanceSoundRing;



    #region callbacks
    protected override void Start(){
        base.Start();
        controller = this.GetComponent<CharacterController>();
        
        SetStateDriver(new IdleMoveState(this, animator)); //player starts unaggro idle move state

        CurrMovementSpeed = (characterdata as PlayerData).jogSpeed; 

        if(gameObject.layer != LayerMask.NameToLayer("Player")) Debug.LogWarning ("layer should be set to Player, not " + LayerMask.LayerToName(gameObject.layer));


        //stealth ring stuff
        StealthRing = GameObject.FindGameObjectWithTag("StealthRing");
        StealthRing.SetActive(false);
        
    }

    protected override void Update() {
        base.Update(); 

        try{ 
            OnInventoryUpdate(); //idk how performant - maybe shouldnt be looped
        } catch {} //temporarily ignore if no hud - this is terrible practice

        if(!InDialogue){ //lock out any controlls if in dialogue
            inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")); //get actual input
            inputVector = Camera.main.transform.TransformDirection(inputVector); //adjust based on rotated camera angle
            inputVector.y = 0f; //no jumping and shit
            DetermineInputOutcome(); 
        }
    }

    //update inventory UI with current inventory information
    protected void OnInventoryUpdate(){
        InventorySlot healthSlot = inventory.FindInInventory("HealthPot");
        InventorySlot staminaSlot = inventory.FindInInventory("StaminaPot");
        healthPotCount.SetText(healthSlot == null ? "0" : healthSlot.quantity.ToString());
        staminaPotCount.SetText(staminaSlot == null ? "0" : staminaSlot.quantity.ToString());
    }

    void FixedUpdate() {
        //fix directions to be right length - all speed dependencies occur AFTER this
        inputVector.Normalize();

        //only move under these conditions
        if (!(genericState is DodgeState) || !(genericState is AttackState) || !(genericState is FollowUpState)) {
           if((inputVector.x != 0 || inputVector.z != 0) && OnSlope()) { //deals with slope velocity
                controller.Move( (inputVector * CurrMovementSpeed * Time.fixedDeltaTime) + (Vector3.down * controller.height / 2 * slopeForce) );
            } else {
                controller.SimpleMove(inputVector * CurrMovementSpeed);  
            }
        } 

    }

    void LateUpdate() {
        CalculateVelocity(); //for the purpose of blending animations
        //TiltOnDelta();
        
    }

    Vector2 preDir, curDir; 
    float tiltVel;
    //not used atm
    protected void TiltOnDelta() {
        //get neccesary info this time around
        Vector2 curDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Camera.main.transform.TransformDirection(curDir);
        //Debug.Log($"pre: {preDir}, post: {curDir}");

        //get the smoothed delta
        float xDiff = curDir.x < 0 ? preDir.x - curDir.x : curDir.x - preDir.x; xDiff *= 120;
        float zDiff = curDir.y < 0 ? preDir.y - curDir.y : curDir.y - preDir.y; zDiff *= 120;
        //translate said delta into lerped tilt with high smooth
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(xDiff + zDiff, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z), Time.deltaTime * 3);

        //update the last dir for info
        preDir = curDir;
    }

    Vector3 preVelocity, velVel, currVelocity;
    protected void CalculateVelocity(){ //i clculate my own velocity cause unity's controller velocity is dogshit
        currVelocity = Vector3.SmoothDamp(preVelocity, controller.velocity, ref velVel, .2f);

        preVelocity = currVelocity;
        animator.SetFloat(Animator.StringToHash("PlayerSpeed"), currVelocity.magnitude);

       // Debug.Log(currVelocity + ", old: " + preVelocity);
    }
    //for slope speed
    private bool OnSlope(){
        RaycastHit hit;
        return Physics.Raycast(transform.position, Vector3.down, out hit, controller.height/2 * slopeForceRayLength) && hit.normal != Vector3.up;
    }
    //for stealth reactoin time
    public void ChangeStanceStealthConsequences(float stanceModifier, float soundModify){
        OnStanceChangeTimer?.Invoke(stanceModifier);
        OnStanceSoundRing?.Invoke(soundModify);
    }
    

    #endregion

    #region chieftan fish
    //master if statement
    //if anywhere, this is where u start crying
    //all movement keys, stance changes, etc, occur here
    private void DetermineInputOutcome() {
         if(genericState is MoveState) { 
            //check for sheathing
            if(Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Fire1")) { 
                SetStateDriver(new SheathingCombatState(this, animator));
            } else {
                HandleNormalMovement();
                FaceKeyPress();
            }
        } else {  //combat state (implied)
            //check for sheathing  
            if(Input.GetKeyDown(KeyCode.X)) {
                if(genericState is SheathingCombatState) {
                    (genericState as SheathingCombatState).ToggleAnim(false);
                } else {
                    SetStateDriver(new SheathingCombatState(this, animator, true));
                }
            //check for crouch sheathing
            } else if(Input.GetKeyDown(KeyCode.C)) {
                if(!animator.GetBool(Animator.StringToHash("Crouching"))){
                    if(genericState is SheathingCombatState) {
                        (genericState as SheathingCombatState).ToggleAnim(true);
                    } else {
                        SetStateDriver(new SheathingCombatState(this, animator, true, true));
                    }
                }
            } else if(!(genericState is AttackState) && !(genericState is FollowUpState)) {  //dont allow any movement if mid swing
                    if(!(genericState is DodgeState)){ 
                        if(animator.GetBool(Animator.StringToHash("Crouching"))) { //no blending crouch animations availbe so just face key press
                            FaceKeyPress();
                        } else {
                            FaceMouseDirection();
                            if(!(genericState is SheathingCombatState)) HandleCombatInteractions(); //no attacking when sheathing
                        }
                    }
                    HandleCombatMovement(); //feet direction info
                }
            
            }

        HandleNormalInteractions();  
    }

    #endregion

    protected override bool TakeDamageAndCheckDeath(float damage, bool isStaggerable, CharacterHandler attacker){
        bool f = base.TakeDamageAndCheckDeath(damage, isStaggerable, attacker);
        //store cause this method changes Health - probably terrible practice

        if(Health <= 0) { //UPON DEATH - destroy the player (temporary)
            this.gameObject.SetActive(false);
        }

        
        return f;
    }


    #region big fish
    private void HandleNormalInteractions(){
        if(Input.GetKeyDown(KeyCode.F)) {
            OnInteract?.Invoke();
        }

        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            inventory.UseItem("HealthPot", this);
        }

        if(Input.GetKeyDown(KeyCode.Alpha2)) {
            inventory.UseItem("StaminaPot", this);
        }
    }

    //this looks more intimating that it actually is
    //every genericState has an appropriate transition - 
    //ex, 
        //if im idle and press w, i should start jogging
        //if im jogging and press shift, i should start sprinting
        //if im crouching and press c, i should stand up 
        //etc.

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
            //localDir = transform.InverseTransformDirection(dodgeDirection).normalized;
            animator.SetFloat(Animator.StringToHash("XMove"), dodgeDirection.x);
            animator.SetFloat(Animator.StringToHash("ZMove"), dodgeDirection.z);

        } else { //if i am not dodging
            //if i have activated dodged, do the appropriate setup

            //note - the set 4 directions are by kevinG's behest - subject to change
            if(Input.GetButtonDown("Jump") && (inputVector.x != 0 || inputVector.z != 0) && Stamina > 0) {                
                //get the GLOBAL direction the player is facing
                //from this, derive the four PLAYER DIRECTION RELATIVE dodge directions
                Vector3[] possibleDodgeDirs = new Vector3[4];

                possibleDodgeDirs[0] = transform.forward;
                possibleDodgeDirs[1] = transform.right;
                possibleDodgeDirs[2] = -transform.forward;
                possibleDodgeDirs[3] = -transform.right;

                //compare CAMERA RELATIVE input direction to the four, find the closest one
                localDir = transform.InverseTransformDirection(inputVector).normalized;
                
                int maxDotIndex = 0;
                float maxDot = Vector3.Dot(localDir, possibleDodgeDirs[0]);
                for(int i = 1; i < 4; ++i){
                    float tempDot = Vector3.Dot(localDir, possibleDodgeDirs[i]);
                    if(tempDot > maxDot) {
                        maxDot = tempDot;
                        maxDotIndex = i;
                    }
                }
                //go that way


                //Debug.Log(possibleDodgeDirs[maxDotIndex]);
                dodgeDirection = possibleDodgeDirs[maxDotIndex]; //save info for movement during dodge
                SetStateDriver(new DodgeState(this, animator));                
                
                //set dodge dirs
                animator.SetFloat(Animator.StringToHash("XMove"), dodgeDirection.x);
                animator.SetFloat(Animator.StringToHash("ZMove"), dodgeDirection.z);


            } else {
                //get local dir from currVelocity instead
                localDir = transform.InverseTransformDirection(currVelocity).normalized;

                //set the floats for movement directions
                float weight = Mathf.InverseLerp(0, (characterdata as PlayerData).combatMoveSpeed, currVelocity.magnitude);
                animator.SetFloat(Animator.StringToHash("XMove"), localDir.x * weight);
                animator.SetFloat(Animator.StringToHash("ZMove"), localDir.z * weight);

            }
        }

        
    
       // Debug.Log(currVelocity);
    }

    //for attacking and blocking
    private void HandleCombatInteractions() {
        if(genericState is DefaultCombatState || genericState is FollowUpState) {
            if(Input.GetButtonDown("Fire1") == true) {
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
   
    public void SheathParentManagement(){
        if(animator.GetFloat(Animator.StringToHash("SheathDir")) < 0){
            ParentToHand();
        } else {
            ParentToSheath();
        }
    }

    public void UnsheathParentManagement(){
        if(animator.GetFloat(Animator.StringToHash("SheathDir")) > 0){
            ParentToHand();
        } else {
            ParentToSheath();
        }    
    }

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
