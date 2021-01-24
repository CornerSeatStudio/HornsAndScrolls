using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

//CombatState represents when a character is in a combat mode. 
//CombatState and MoveState are switched between in the PlayerHandler

public class SheathingCombatState : CombatState {
    IEnumerator sheath;
    float animTime = .9f; //sheath time (MANUALLY CODED BUT WHY NOT LOL)
    //ALSO NOTE: this has to be LONGEr than the ACTUAL ANIMATION cause COUPLING
    float currAnimTime = 0f;
    bool isSheathing = false;
    bool crouchSheathing = false;
    
    public SheathingCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}
    public SheathingCombatState(CharacterHandler character, Animator animator, bool isSheathing) : base(character, animator) {
        this.isSheathing = isSheathing;
    }
    public SheathingCombatState(CharacterHandler character, Animator animator, bool isSheathing, bool crouchSheathing) : base(character, animator) {
        this.isSheathing = isSheathing;
        this.crouchSheathing = crouchSheathing;
    }

    public override IEnumerator OnStateEnter() {

        //fuck it just set layer - if it cucks up in the future use above
        animator.SetLayerWeight(1, 1);
        

        (character as PlayerHandler).ChangeStanceStealthConsequences((character.characterdata as PlayerData).detectionTime, 9f); //stealth stuff

        animator.SetBool(Animator.StringToHash("Combat"), true); //for facing mouse animation
        animator.SetBool(Animator.StringToHash("midDraw"), true);//determines when to actually transition out
        animator.SetFloat(Animator.StringToHash("SheathDir"), 1); //speed of anim (-1 to reverse it)

        animator.SetBool(Animator.StringToHash("Crouching"), crouchSheathing ? true : false);


        //sound and animation direction
        AudioData nextSound = isSheathing ? Array.Find(character.audioData, AudioData => AudioData.name == "sheath") : Array.Find(character.audioData, AudioData => AudioData.name == "unsheath");
        nextSound.Play(character.AudioSource);

        //start the RIGHT animation depending on where its coming from
        sheath = isSheathing ? Sheath() : Unsheath();

        yield return character.StartCoroutine(sheath);


    }


    float handThreshold;
    private IEnumerator Unsheath(){
        //while still in range of time
        animator.SetTrigger(Animator.StringToHash("Unsheath")); //begin the animation

        //(character as PlayerHandler).ParentToHand(); //parent to hand

        while(currAnimTime >= 0 && currAnimTime <= animTime) {
            currAnimTime += isSheathing ? -.1f : .1f;
            yield return new WaitForSeconds(.1f);
        }

        //one done, determine outcome
        if(currAnimTime <= 0) {
            animator.SetBool(Animator.StringToHash("midDraw"), false); //drawing is finished
            animator.SetBool(Animator.StringToHash("Combat"), false);
            yield return new WaitForSeconds(.2f);

            animator.SetLayerWeight(1, 0);
            character.SetStateDriver(new IdleMoveState(character, animator));
        } else {
            animator.SetBool(Animator.StringToHash("midDraw"), false); //drawing is finished
            animator.SetBool(Animator.StringToHash("Combat"), true); //drawing is finished
            
            yield return new WaitForSeconds(.2f); //allow anim transition to work a bit first god damn i hate this job
            character.SetStateDriver(new DefaultCombatState(character, animator));
        }

    }

    private IEnumerator Sheath(){
        animator.SetTrigger(Animator.StringToHash("Sheath")); //begin the animation

        //while still in range of time
        while(currAnimTime >= 0 && currAnimTime <= animTime) {
            currAnimTime += isSheathing ? .1f : -.1f;
            yield return new WaitForSeconds(.1f);
        }

        //Debug.Log("yewot");

        //one done, determine outcome
        if(currAnimTime <= 0) {
            character.SetStateDriver(new DefaultCombatState(character, animator));
        } else {
            animator.SetBool(Animator.StringToHash("midDraw"), false); //drawing is finished
            animator.SetBool(Animator.StringToHash("Combat"), false);
            
            yield return new WaitForSeconds(.2f);
            animator.SetLayerWeight(1, 0);

            if(crouchSheathing) {
                character.SetStateDriver(new CrouchIdleMoveState(character, animator));
            } else {
                character.SetStateDriver(new IdleMoveState(character, animator));
            }
        }
    }

    public void ToggleAnim(bool toCrouch){ //switch sheath direction, animation playback speed, and sound start management
        isSheathing = !isSheathing;
        animator.SetFloat(Animator.StringToHash("SheathDir"), animator.GetFloat(Animator.StringToHash("SheathDir")) * -1f);
        AudioData nextSound = isSheathing ? Array.Find(character.audioData, AudioData => AudioData.name == "sheath") : Array.Find(character.audioData, AudioData => AudioData.name == "unsheath");

        animator.SetBool(Animator.StringToHash("Crouching"), toCrouch);

        nextSound.Play(character.AudioSource);

    }

    float currWeight, timeVal;

    public override IEnumerator OnStateExit() { //once drawn OR INTERUPTED
        if(sheath != null) character.StopCoroutine(sheath);
        yield break;
    }
}

public class DefaultCombatState : CombatState {
    public DefaultCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        try {
            (character as PlayerHandler).CurrMovementSpeed = (character.characterdata as PlayerData).combatMoveSpeed;

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
        character.SetStateDriver(new FollowUpState(character, animator, chosenMove.endlag));
        //TODO GO SOMWHERE ELSE
    }    

    private IEnumerator AttackTime(){
        finishedAttack = false;
        yield return new WaitForSeconds(chosenMove.endlag * .8f);
        finishedAttack = true;
        timerRoutine = null;
    }

    private IEnumerator Attack(){
                // Debug.Log(chosenMove.name);

        //begin animation
        animator.applyRootMotion = true; //cleaner maybe
        animator.SetTrigger(Animator.StringToHash(chosenMove.name));
        if(character is PlayerHandler) character.WeaponTrail.enabled = true;
        //sound
        
        //time it takes before weapon trigger is allowed to do damage
        yield return new WaitForSeconds(chosenMove.startup);

        try { Array.Find(character.audioData, AudioData => AudioData.name == "woosh").Play(character.AudioSource); } catch {} //temp todo

        //timer to finish attack if no contact
        timerRoutine = AttackTime();
        character.StartCoroutine(timerRoutine);

        //two condtions: either contact with target, or attack is done swinging
        yield return new WaitUntil(() => finishedAttack || character.CanAttack);

        //if attack is succesful, set response
        if(character.CanAttack) character.AttackRequest(chosenMove.damage);

    }
    public override IEnumerator OnStateExit() {
        if(character is PlayerHandler) character.WeaponTrail.enabled = false;

        if(timerRoutine != null) character.StopCoroutine(timerRoutine);
        if(currAttackCoroutine != null) character.StopCoroutine(currAttackCoroutine); 
        yield break;
    }
}

//grace period for comboing
public class FollowUpState : CombatState {

    IEnumerator timeo;
    float endlag;
    public FollowUpState(CharacterHandler character, Animator animator, float endlag) : base(character, animator) {
        this.endlag = endlag;
    }

    public override IEnumerator OnStateEnter(){
        timeo = FollowUpTime();
        yield return character.StartCoroutine(timeo);
        character.SetStateDriver(new DefaultCombatState(character, animator));

    }

    private IEnumerator FollowUpTime(){
        yield return new WaitForSeconds(endlag * .4f);
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

    public override IEnumerator OnStateExit() { 
       if(blockStaminaDrain != null) character.StopCoroutine(blockStaminaDrain);
        animator.SetBool(Animator.StringToHash("Blocking"), false);
        yield return new WaitForSeconds(.05f);
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


        animator.SetBool(Animator.StringToHash("Blocking"), true);
        yield return new WaitForSeconds(0.3f); //where param is counter timeframe
        character.SetStateDriver(new BlockState(character, animator));
    }

    float currWeight, timeVal;
    
    public override IEnumerator OnStateExit() {
        yield break;
    }

}

public class DodgeState : CombatState {

    public DodgeState(CharacterHandler character, Animator animator) : base(character, animator) {}


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
 
        animator.SetTrigger(Animator.StringToHash("Stagger"));
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

            //remove from combat list
            AIHandler.CombatAI.Remove(character as AIHandler);

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