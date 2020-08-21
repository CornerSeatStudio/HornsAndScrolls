using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class SheathingCombatState : CombatState {
    IEnumerator sheath, layerRoutine;
    float animTime = 1.2f; //sheath time
    float currAnimTime = 0f;
    float currAudioTime = 0f;
    bool isSheathing = false;
    
    public SheathingCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}
    public SheathingCombatState(CharacterHandler character, Animator animator, bool isSheathing) : base(character, animator) {
        this.isSheathing = isSheathing;
    }

    public override IEnumerator OnStateEnter() {

        //layer shifto
        // layerRoutine = LayerUp();
        // yield return character.StartCoroutine(layerRoutine);
        //fuck it just set layer - if it cucks up in the future use above
        animator.SetLayerWeight(1, 1);
        

        (character as PlayerHandler).ChangeStanceTimer((character.characterdata as PlayerData).detectionTime); //stealth stuff

        animator.SetBool(Animator.StringToHash("Combat"), true); //for facing mouse animation
        animator.SetBool(Animator.StringToHash("midDraw"), true);//determines when to actually transition out
        animator.SetFloat(Animator.StringToHash("SheathDir"), 1); //speed of anim (-1 to reverse it)
        animator.SetBool(Animator.StringToHash("Crouching"), false); //if coming from crouch sheath case

        //sound and animation direction
        AudioData nextSound = isSheathing ? Array.Find(character.audioData, AudioData => AudioData.name == "sheath") : Array.Find(character.audioData, AudioData => AudioData.name == "unsheath");
        nextSound.Play(character.AudioSource);

        //start the RIGHT animation depending on where its coming from
        sheath = isSheathing ? Sheath() : Unsheath();

        yield return character.StartCoroutine(sheath);


    }



    private IEnumerator Unsheath(){
        //while still in range of time
        animator.SetTrigger(Animator.StringToHash("Unsheath")); //begin the animation

        (character as PlayerHandler).ParentToHand(); //parent to hand

        while(currAnimTime >= 0 && currAnimTime <= animTime) {
            currAnimTime += isSheathing ? -.1f : .1f;
            currAudioTime += .1f;
            yield return new WaitForSeconds(.1f);
        }

        //one done, determine outcome
        if(currAnimTime <= 0) {
            try { (character as PlayerHandler).ParentToSheath(); } catch { Debug.LogWarning("ai shoudnt be in this state"); }
            character.SetStateDriver(new IdleMoveState(character, animator));
        } else {
            character.SetStateDriver(new DefaultCombatState(character, animator));
        }

    }

    private IEnumerator Sheath(){
        animator.SetTrigger(Animator.StringToHash("Sheath")); //begin the animation

        //while still in range of time
        while(currAnimTime >= 0 && currAnimTime <= animTime) {
            currAnimTime += isSheathing ? .1f : -.1f;
            currAudioTime += .1f;
            yield return new WaitForSeconds(.1f);
        }

        //one done, determine outcome
        if(currAnimTime <= 0) {
            character.SetStateDriver(new DefaultCombatState(character, animator));
        } else {
            try { (character as PlayerHandler).ParentToSheath(); } catch { Debug.LogWarning("ai shoudnt be in this state"); }
            character.SetStateDriver(new IdleMoveState(character, animator));
        }
    }

    public void ToggleAnim(){ //switch sheath direction, animation playback speed, and sound start management
        isSheathing = !isSheathing;
        animator.SetFloat(Animator.StringToHash("SheathDir"), animator.GetFloat(Animator.StringToHash("SheathDir")) * -1f);
        AudioData nextSound = isSheathing ? Array.Find(character.audioData, AudioData => AudioData.name == "sheath") : Array.Find(character.audioData, AudioData => AudioData.name == "unsheath");
        currAudioTime = nextSound.AverageLength() - currAudioTime;
        nextSound.PlayAtPoint(character.AudioSource, currAudioTime);

    }

    float currWeight, timeVal;
    
    IEnumerator LayerUp(){
        currWeight = timeVal = 0;
        while( Mathf.Abs(currWeight - 1) > 0.01f){
            currWeight = Mathf.Lerp(0, 1, timeVal*3);
            animator.SetLayerWeight(1, currWeight);
            timeVal += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        layerRoutine = null;
        yield break;
        
    }

    IEnumerator LayerDown(){ //purposefully slow for juke reasons lmao
        currWeight = 1;
        timeVal = 0;
        while(Mathf.Abs(currWeight) > 0.01f && !(character.genericState is SheathingCrouchState)) { //stop if state has changed to crouch sheath
            currWeight = Mathf.Lerp(1, 0, timeVal*3);
            animator.SetLayerWeight(1, currWeight);
            timeVal += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        layerRoutine = null;
        yield break;
    }

    public override IEnumerator OnStateExit() { //once drawn OR INTERUPTED
        if(sheath != null) character.StopCoroutine(sheath);
        if(layerRoutine != null) character.StopCoroutine(layerRoutine);

        layerRoutine = LayerDown();
        character.StartCoroutine(layerRoutine);

        yield break;
    }
}
public class SheathingCrouchState : MoveState {
    IEnumerator sheath, layerRoutine;
    float animTime = 1.2f; //sheath time
    float currAnimTime = 0f;
    float currAudioTime = 0f;

    public SheathingCrouchState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        animator.SetLayerWeight(1, 1);


        (character as PlayerHandler).ChangeStanceTimer((character.characterdata as PlayerData).detectionTime * 1.5f); //stealth stuff

        animator.SetBool(Animator.StringToHash("Combat"), false); //for crouch case
        animator.SetBool(Animator.StringToHash("Crouching"), true); //for crouch case

        animator.SetBool(Animator.StringToHash("midDraw"), true);//determines when to actually transition out
        animator.SetFloat(Animator.StringToHash("SheathDir"), 1); //speed of anim 

        Array.Find(character.audioData, AudioData => AudioData.name == "sheath").Play(character.AudioSource);

        //start the sheath animation depending on where its coming from
        animator.SetTrigger(Animator.StringToHash("Sheath")); //begin the animation
        sheath = Sheath();
        yield return character.StartCoroutine(sheath);


    }

    private IEnumerator Sheath(){
        //while still in range of time
        while(currAnimTime <= animTime) {
            currAnimTime += .1f;
            currAudioTime += .1f;
            yield return new WaitForSeconds(.1f);
        }

        //one done, idle outcome
        try { (character as PlayerHandler).ParentToSheath(); } catch { Debug.LogWarning("ai shoudnt be in this state"); }
        character.SetStateDriver(new CrouchIdleMoveState(character, animator));
        
    }

    float currWeight, timeVal;
    IEnumerator LayerDown(){ //purposefully slow for juke reasons lmao
        currWeight = 1;
        timeVal = 0;
        while(Mathf.Abs(currWeight) > 0.01f && !(character.genericState is SheathingCombatState)) {
            currWeight = Mathf.Lerp(1, 0, timeVal*3);
            animator.SetLayerWeight(1, currWeight);
            timeVal += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        layerRoutine = null;
        yield break;
    }

    public override IEnumerator OnStateExit() { //once drawn OR INTERUPTED
        if(sheath != null) character.StopCoroutine(sheath);

        layerRoutine = LayerDown();
        character.StartCoroutine(layerRoutine); //TODO THIS SHOULD BE STOPPED IF DONE IN AGAIN


        yield break;
    }
}

public class DefaultCombatState : CombatState {
    public DefaultCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        try {
            (character as PlayerHandler).CurrMovementSpeed = (character.characterdata as PlayerData).combatMoveSpeed;
            animator.SetBool(Animator.StringToHash("midDraw"), false); //drawing is finished

        } catch {
            //Debug.Log("not a player");
        }

        yield break;
    }

}

public class AttackState : CombatState {
    protected IEnumerator currAttackCoroutine;
    private IEnumerator timerRoutine;
    public MeleeMove chosenMove {get; private set;}
    private bool finishedAttack;
    public AttackState(CharacterHandler character, Animator animator) : base(character, animator) {
        try {chosenMove = character.MeleeAttacks["default"]; } catch { Debug.LogWarning("some cunt don't have default attack"); }
    }

    public AttackState(CharacterHandler character, Animator animator, MeleeMove chosenMove) : base(character, animator) {
        this.chosenMove = chosenMove;
    }
    public override IEnumerator OnStateEnter() { 
       currAttackCoroutine = Attack();
       yield return character.StartCoroutine(currAttackCoroutine);
       
        //wrap it up
        character.SetStateDriver(new FollowUpState(character, animator));
        //TODO GO SOMWHERE ELSE
    }    

    private IEnumerator AttackTime(){
        finishedAttack = false;
        yield return new WaitForSeconds(chosenMove.endlag);
        finishedAttack = true;
        timerRoutine = null;
    }

    private IEnumerator Attack(){
        //begin animation
        animator.applyRootMotion = true; //cleaner maybe
        //yield return new WaitForSeconds(.5f);

        animator.SetTrigger(Animator.StringToHash(chosenMove.name));

        //sound
        try { Array.Find(character.audioData, AudioData => AudioData.name == "woosh").Play(character.AudioSource); } catch {} //temp todo
        
        //time it takes before weapon trigger is allowed to do damage
        yield return new WaitForSeconds(chosenMove.startup);
        //timer to finish attack if no contact
        timerRoutine = AttackTime();
        character.StartCoroutine(timerRoutine);

        //two condtions: either contact with target, or attack is done swinging
        yield return new WaitUntil(() => finishedAttack || character.CanAttack);

        //if attack is succesful, set response
        if(character.CanAttack) character.AttackRequest(chosenMove.damage);

    }
    public override IEnumerator OnStateExit() {
      
        if(timerRoutine != null) character.StopCoroutine(timerRoutine);
        if(currAttackCoroutine != null) character.StopCoroutine(currAttackCoroutine); 
        yield break;
    }
}

//grace period for comboing
public class FollowUpState : CombatState {

    IEnumerator timeo;
    public FollowUpState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter(){
        timeo = FollowUpTime();
        yield return character.StartCoroutine(timeo);
        character.SetStateDriver(new DefaultCombatState(character, animator));

    }

    private IEnumerator FollowUpTime(){
        yield return new WaitForSeconds(.4f);
    }

    public override IEnumerator OnStateExit(){
        if(timeo != null) character.StopCoroutine(timeo);
        animator.applyRootMotion = false;
        yield break;
    }


}

public class BlockState : CombatState {
    protected MeleeMove block;
    IEnumerator blockStaminaDrain, layerRoutine;

    public BlockState(CharacterHandler character, Animator animator) : base(character, animator) {
        try {block = character.MeleeBlock; } catch { Debug.LogWarning("no block in char SO"); }
    }

    public override IEnumerator OnStateEnter() { 
        animator.SetBool(Animator.StringToHash("Blocking"), true);
        blockStaminaDrain = StaminDrainOverTime();
        character.StartCoroutine(blockStaminaDrain);
        yield break;
    } 

    private IEnumerator StaminDrainOverTime(){
        while(true){
            character.DealStamina(.5f);
            yield return new WaitForSeconds(.2f);
        }
    }

    float currWeight, timeVal;

    IEnumerator LayerDown(){ //purposefully slow for juke reasons lmao
        currWeight = 1;
        timeVal = 0;
        while(Mathf.Abs(currWeight) > 0.01f) { //stop if state has changed to crouch sheath
            currWeight = Mathf.Lerp(1, 0, timeVal *3);
            animator.SetLayerWeight(1, currWeight);
            timeVal += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        layerRoutine = null;
        yield break;
    }

    public override IEnumerator OnStateExit() { 
       if(blockStaminaDrain != null) character.StopCoroutine(blockStaminaDrain);
        animator.SetBool(Animator.StringToHash("Blocking"), false);

        if(character is PlayerHandler){
            layerRoutine = LayerDown();
            character.StartCoroutine(layerRoutine);
        }
        yield break;
    } 

}

public class CounterState : CombatState {
    protected MeleeMove block;
    IEnumerator layerRoutine;

    public CounterState(CharacterHandler character, Animator animator) : base(character, animator) {
        try {block = character.MeleeBlock; } catch { Debug.LogWarning("no block in char SO"); }
    }

    public override IEnumerator OnStateEnter() {   
        //trigger counter event
        //if enemy is attacking you specifically (dont trigger if coming from a specific state)
        //shouldnt be done here, should be done in attack response via comparing type
        animator.SetLayerWeight(1, 1);


        animator.SetBool(Animator.StringToHash("Blocking"), true);
        yield return new WaitForSeconds(0.3f); //where param is counter timeframe
        character.SetStateDriver(new BlockState(character, animator));
    }

    float currWeight, timeVal;
    
    IEnumerator LayerDown(){ //purposefully slow for juke reasons lmao
        currWeight = 1;
        timeVal = 0;
        while(Mathf.Abs(currWeight) > 0.01f) { //stop if state has changed to crouch sheath
            currWeight = Mathf.Lerp(1, 0, timeVal *3);
            animator.SetLayerWeight(1, currWeight);
            timeVal += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        layerRoutine = null;
        yield break;
    }
    public override IEnumerator OnStateExit() {
        if(layerRoutine != null) {
            character.StopCoroutine(layerRoutine);
            layerRoutine = LayerDown();
            character.StartCoroutine(layerRoutine);
        }
        yield break;
    }

}

public class DodgeState : CombatState {
    Vector3 direction;

    public DodgeState(CharacterHandler character, Animator animator, Vector3 direction) : base(character, animator) {
        this.direction = direction;
    }


    public override IEnumerator OnStateEnter() {
        animator.ResetTrigger(Animator.StringToHash("Dodge"));
        animator.SetTrigger(Animator.StringToHash("Dodge"));
        character.DealStamina(3f);
        animator.applyRootMotion = true;
        yield return new WaitForSeconds((character.characterdata as PlayerData).dodgeTime);
        character.SetStateDriver(new DefaultCombatState(character, animator));
    }



    public override IEnumerator OnStateExit() {
        animator.applyRootMotion = false;
        yield break;
    }

}

public class StaggerState : CombatState {
    public StaggerState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {  
        Array.Find(character.audioData, AudioData => AudioData.name == "stagger").Play(character.AudioSource);
 
        animator.SetTrigger(Animator.StringToHash("Staggering"));
        yield return new WaitForSeconds(.7f); //stagger time
        character.SetStateDriver(new DefaultCombatState(character, animator));      
    }

    public override IEnumerator OnStateExit() {
        yield break;
   
    }


}

public class DeathState : CombatState {
    public DeathState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter(){
        Array.Find(character.audioData, AudioData => AudioData.name == "death").Play(character.AudioSource);

        //change globalState
        try{
            (character as AIHandler).GlobalState = GlobalState.DEAD;
            //stop APPROPRITE coroutines
            (character as AIHandler).Detection.IsAlive = false; //detection
            //weapon stuff
            (character as AIHandler).weaponHitbox.transform.SetParent(null);
            (character as AIHandler).weaponHitbox.isTrigger = false;
            //ragdoll 
            (character as AIHandler).GetComponent<Collider>().enabled = false;
            (character as AIHandler).GetComponent<NavMeshAgent>().enabled = false;
        } catch {
            Debug.LogWarning("player shouldnt use death state for now");
        }
        
        Rigidbody[] rigidbodies = character.GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in rigidbodies){
            if(rb.gameObject != character.gameObject){
                rb.isKinematic = false;
            }
        }

        Collider[] colliders = character.GetComponentsInChildren<Collider>();
        foreach(Collider col in colliders) {
            if (col.gameObject != character.gameObject) {
                col.enabled = true;
            } 
        }

        //disable animator as last step
        animator.enabled = false;
        Debug.Log("agh i haveth been a slain o");
        yield return null;
    }

    public override IEnumerator OnStateExit() {
        Debug.Log("exiting death state?");
        yield break;

    }

}