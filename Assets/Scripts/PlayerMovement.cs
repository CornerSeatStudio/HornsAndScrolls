using System.Collections;
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
    
    public Dictionary<string, int> AnimationHashes { get; private set; }

    void Start() {
        animator = this.GetComponent<Animator>();
        controller = this.GetComponent<CharacterController>();
        setupAnimationHashes();
    }

    private void setupAnimationHashes() {
        AnimationHashes = new Dictionary<string, int>();
        AnimationHashes.Add("IsSprinting", Animator.StringToHash("IsSprinting"));
        AnimationHashes.Add("IsSlowWalking", Animator.StringToHash("IsSlowWalking"));
        AnimationHashes.Add("directionofdodgeFB", Animator.StringToHash("directionofdodgeFB"));
        AnimationHashes.Add("directionofdodgeLR", Animator.StringToHash("directionofdodgeLR"));
        AnimationHashes.Add("IsDodging", Animator.StringToHash("IsDodging"));
        AnimationHashes.Add("IsWalking", Animator.StringToHash("IsWalking"));
        AnimationHashes.Add("IsWalkingBack", Animator.StringToHash("IsWalkingBack"));
        AnimationHashes.Add("IsCrouching", Animator.StringToHash("IsCrouching"));
        AnimationHashes.Add("IsWalkingRight", Animator.StringToHash("IsWalkingRight"));
        AnimationHashes.Add("IsCrouchingWalking", Animator.StringToHash("IsCrouchingWalking"));
        AnimationHashes.Add("IsDodgingRight", Animator.StringToHash("IsDodgingRight"));
        AnimationHashes.Add("IsDodgingLeft", Animator.StringToHash("IsDodgingLeft"));
        AnimationHashes.Add("IsWeaponout", Animator.StringToHash("IsWeaponout"));
        AnimationHashes.Add("IsWalkingLeft", Animator.StringToHash("IsWalkingLeft"));
        AnimationHashes.Add("IsDodgingBack", Animator.StringToHash("IsDodgingBack"));
        
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

        //my normilization fuck the normal stuff
        if(inputVector.x !=0 && inputVector.z!=0){
            inputVector = new Vector3(Input.GetAxisRaw("Horizontal")*movementSpeed*0.7f, 0f, Input.GetAxisRaw("Vertical")*movementSpeed*0.7f);
            //Debug.Log(inputVector);
        }

        //only 


        //Crouching if statement
        //if pressing C, am currently not crouching, am not moving, and am not sprinting
        if(Input.GetKeyDown(KeyCode.C)&&!IsCrouching&&(inputVector.x == 0 || inputVector.z == 0)&&!IsSprinting){
            IsCrouching=true;
        }else if(Input.GetKeyDown(KeyCode.C)&&IsCrouching){
            IsCrouching=false;
        }

        //Crouch walking if statement
        //if i am moving, not sprinting, and crouching
        if((inputVector.x != 0 || inputVector.z != 0)&&!IsSprinting&&IsCrouching){
            IsCrouchingWalking = true;
            movementSpeed = 10f;
            IsWeaponout=false;

        }else if(inputVector.x ==0 && inputVector.z == 0){
            IsCrouchingWalking=false;
        }

        //sprinting if statements
        //if i am pressing left shift, am not crouching, and am not crouch walking
        if(Input.GetKeyDown(KeyCode.LeftShift)&&!IsCrouching&&!IsCrouchingWalking){
            movementSpeed=25f;
            IsSprinting=true;
            IsWeaponout=false;
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift)){
            IsSprinting=false;
        }


        //slow walking if statements
        if(Input.GetKeyDown(KeyCode.Z)){
            IsSlowWalking=true;
            movementSpeed=5f;
        }
        else if(Input.GetKeyUp(KeyCode.Z)){
            IsSlowWalking=false;
        }
        
        //sheathe weapon
        //if i am pressing x, the weapoin isnt out, i am not sprinting nor crouching
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
            }
            if(directionofdodgeFB<0){
                IsDodgingBack=true;
            }
            if(directionofdodgeLR>0){
                IsDodgingRight=true;
            }
            if(directionofdodgeLR<0){
                IsDodgingLeft=true;
            }
            rachedtimer=30f;
        }else if(rachedtimer==0&&(IsDodging||IsDodgingBack||IsDodgingLeft||IsDodgingRight)){
            IsDodging=false;
            IsDodgingBack=false;
            IsDodgingLeft=false;
            IsDodgingRight=false;
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
        animator.SetBool(AnimationHashes["IsWalking"], IsWalking);
        animator.SetBool(AnimationHashes["IsSprinting"], IsSprinting);
        animator.SetBool(AnimationHashes["IsSlowWalking"], IsSlowWalking);
        animator.SetBool(AnimationHashes["IsCrouching"], IsCrouching);
        animator.SetBool(AnimationHashes["IsCrouchingWalking"], IsCrouchingWalking);
        animator.SetBool(AnimationHashes["IsWalkingBack"], IsWalkingBack);
        animator.SetBool(AnimationHashes["IsWeaponout"], IsWeaponout);
        animator.SetBool(AnimationHashes["IsWalkingRight"], IsWalkingRight);
        animator.SetBool(AnimationHashes["IsWalkingLeft"], IsWalkingLeft);
        animator.SetBool(AnimationHashes["IsDodging"], IsDodging);
        animator.SetBool(AnimationHashes["IsDodgingBack"], IsDodgingBack);
        animator.SetBool(AnimationHashes["IsDodgingRight"], IsDodgingRight);
        animator.SetBool(AnimationHashes["IsDodgingLeft"], IsDodgingLeft);

    }
    
}