using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class SheathingCombatState : CombatState {
    IEnumerator sheath;
    float animTime = 1.2f; //sheath time
    float currAnimTime = 0f;
    float currAudioTime = 0f;
    bool isSheathing = false;
    public SheathingCombatState(CharacterHandler character, Animator animator) : base(character, animator) {}
    public SheathingCombatState(CharacterHandler character, Animator animator, bool isSheathing) : base(character, animator) {
        this.isSheathing = isSheathing;
    }

    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).ChangeStanceTimer(1f); //stealth stuff

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

    public override IEnumerator OnStateExit() { //once drawn OR INTERUPTED
        if(sheath != null) character.StopCoroutine(sheath);
        
        // character.layerWeightRoutine = character.LayerWeightDriver(1, 1, 0, .3f);
        // yield return character.StartCoroutine(character.layerWeightRoutine);
        yield break;
    }
}
public class SheathingCrouchState : MoveState {
    IEnumerator sheath;
    float animTime = 1.2f; //sheath time
    float currAnimTime = 0f;
    float currAudioTime = 0f;
    public SheathingCrouchState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {
        (character as PlayerHandler).ChangeStanceTimer(1.5f); //stealth stuff

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

    public override IEnumerator OnStateExit() { //once drawn OR INTERUPTED
        if(sheath != null) character.StopCoroutine(sheath);

        // character.layerWeightRoutine = character.LayerWeightDriver(1, 1, 0, .3f);
        // yield return character.StartCoroutine(character.layerWeightRoutine);
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
        character.SetStateDriver(new DefaultCombatState(character, animator));
    }    

    private IEnumerator AttackTime(){
        finishedAttack = false;
        yield return new WaitForSeconds(chosenMove.endlag);
        finishedAttack = true;
        timerRoutine = null;
    }

    private IEnumerator Attack(){
        //begin animation
        animator.SetTrigger(Animator.StringToHash("Attacking"));
        animator.applyRootMotion = true; //cleaner maybe

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
      //  yield return new WaitUntil(() => character.layerWeightRoutine == null);
      //  character.layerWeightRoutine = character.LayerWeightDriver(1, 0, 1, .3f);
        animator.applyRootMotion = false;
     //   yield return character.StartCoroutine(character.layerWeightRoutine);       
       if(timerRoutine != null) character.StopCoroutine(timerRoutine);
        if(currAttackCoroutine != null) character.StopCoroutine(currAttackCoroutine); 
        yield break;
    }
}

public class BlockState : CombatState {
    protected MeleeMove block;
    IEnumerator blockStaminaDrain;

    public BlockState(CharacterHandler character, Animator animator) : base(character, animator) {
        try {block = character.MeleeBlock; } catch { Debug.LogWarning("no block in char SO"); }
    }

    public override IEnumerator OnStateEnter() { 
        animator.SetBool(Animator.StringToHash("Blocking"), true);
        // blockStaminaDrain = StaminDrainOverTime();
        // character.StartCoroutine(blockStaminaDrain);
        yield break;
    } 

    private IEnumerator StaminDrainOverTime(){
        while(true){
            character.DealStamina(.5f);
            yield return new WaitForSeconds(.2f);
        }
    }

    public override IEnumerator OnStateExit() { 
       // if(blockStaminaDrain != null) character.StopCoroutine(blockStaminaDrain);
        animator.SetBool(Animator.StringToHash("Blocking"), false);
        yield break;
    } 

}

public class CounterState : CombatState {
    protected MeleeMove block;

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

    public override IEnumerator OnStateExit() {
        yield break;
    }

}

public class DodgeState : CombatState {
    Vector3 direction;

    public DodgeState(CharacterHandler character, Animator animator, Vector3 direction) : base(character, animator) {
        this.direction = direction;
    }


    public override IEnumerator OnStateEnter() {
        //yield return new WaitUntil(() => character.layerWeightRoutine == null);
        // character.layerWeightRoutine = character.LayerWeightDriver(1, 1, 0, .8f);
        // yield return character.StartCoroutine(character.layerWeightRoutine);  

        animator.ResetTrigger(Animator.StringToHash("Dodge"));
        animator.SetTrigger(Animator.StringToHash("Dodge"));
        yield return new WaitForSeconds((character.characterdata as PlayerData).dodgeTime);
        character.SetStateDriver(new DefaultCombatState(character, animator));
    }

    public override IEnumerator OnStateExit() {
        yield break;
     //   yield return new WaitUntil(() => character.layerWeightRoutine == null);
        // character.layerWeightRoutine = character.LayerWeightDriver(1, 0, 1, .8f);
        // yield return character.StartCoroutine(character.layerWeightRoutine);  
    }

}

public class StaggerState : CombatState {
    public StaggerState(CharacterHandler character, Animator animator) : base(character, animator) {}

    public override IEnumerator OnStateEnter() {  
        Array.Find(character.audioData, AudioData => AudioData.name == "stagger").Play(character.AudioSource);
     //   yield return new WaitUntil(() => character.layerWeightRoutine == null);
      //  character.layerWeightRoutine = character.LayerWeightDriver(1, 1, 0, .2f);
       // yield return character.StartCoroutine(character.layerWeightRoutine);       
        animator.SetTrigger(Animator.StringToHash("Staggering"));
        yield return new WaitForSeconds(.5f); //stagger time
        character.SetStateDriver(new DefaultCombatState(character, animator));      
    }

    public override IEnumerator OnStateExit() {
        yield break;
     //   yield return new WaitUntil(() => character.layerWeightRoutine == null);
        // character.layerWeightRoutine = character.LayerWeightDriver(1, 0, 1, .2f);
        // yield return character.StartCoroutine(character.layerWeightRoutine);       
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
            (character as AIHandler).weaponMesh.transform.SetParent(null);
            (character as AIHandler).weaponMesh.AddComponent<Rigidbody>();
            (character as AIHandler).weaponMesh.AddComponent<BoxCollider>();
            //ragdoll 
            (character as AIHandler).GetComponent<Collider>().enabled = false;
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