﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;

public class CharacterHandler : MonoBehaviour {

    [Header("Core Components/SOs")]
    public CharacterData characterdata;
    public WeaponData weapon; 
    public AudioData[] audioData;

    [Header("Core Members")]
    public Image heathbar;
    public Image staminabar;
    public TextMeshProUGUI debugState; 
    public float staminaRegenerationWindow = 3f;
    
    [Header ("Foliage Handling")]
    public Material[] materials;
    // [Header("IK")]
    // [Range (0, 5)] public float distanceToGround;
    // public LayerMask floor;

    //private stuff
    protected Animator animator;
    protected Vector3 velocity;
    public AudioSource AudioSource {get; private set;}
    public Dictionary<string, MeleeMove> MeleeAttacks {get; private set;} 
    public MeleeMove MeleeBlock {get; private set; }
    public float Health {get; private set; }
    public float Stamina {get; private set; }
    public GenericState genericState {get; protected set;}
    private LayerMask foliageMask;
    

    #region Callbacks
    protected virtual void Start() {
        animator = this.GetComponent<Animator>();        
        AudioSource = this.GetComponent<AudioSource>();
        Health = characterdata.maxHealth;
        Stamina = characterdata.maxStamina;

        
        DisableRagdoll(); //NECCESARY to a. disable ragdoll and b. not fuck up attack script
        PopulateMeleeMoves();

        foliageMask = LayerMask.GetMask("Foliage");
        Renderer rend = GameObject.FindGameObjectsWithTag("Foliage").First().GetComponent<Renderer>();
        StartCoroutine(GrassHandle()); 
        
    }

    protected virtual void Update(){
        if(genericState != null) debugState.SetText(genericState.ToString());
    }

    //disables ragdoll on character on game start
    private void DisableRagdoll() {  
        Rigidbody[] rigidbodies = gameObject.GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in rigidbodies){
            if(rb.gameObject != this.gameObject){
                rb.isKinematic = true;
            }
        }

        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach(Collider col in colliders) {
            if (col.gameObject != this.gameObject) {
                col.enabled = false;
            }
        }
    }

    //organize ALL melee moves moves in dictionary
    private void PopulateMeleeMoves() {
        //for attack
        MeleeAttacks = new Dictionary<string, MeleeMove>();
        foreach(MeleeMove attack in weapon.Attacks) {
            MeleeAttacks.Add(attack.name, attack);
        }

        //for block
        MeleeBlock = weapon.block;
    }
    #endregion

    #region core/var manipulation

        //LayerMasks allow for raycasts to choose what to and not to register
    [Header("find target stuff")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public CharacterHandler FindTarget(MeleeMove meleeMove){
        float minDistanceToTarget = float.MaxValue; //guarantees first check in findingInteractableTargets
        CharacterHandler chosenTarget = null; //reset character chosen
        //cast a sphere over player, store everything inside col
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, meleeMove.range, targetMask);
        
        foreach(Collider col in targetsInView){
            //Debug.Log(col.transform);
            Transform target = col.transform; //get the targets locatoin
            Vector3 directionToTarget = (target.position - transform.position).normalized; //direction vector of where bloke is
            if (Vector3.Angle(transform.forward, directionToTarget) < meleeMove.angle/2){ //if the attack is within bounds, /2 cause left right
                //do the ray 
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)){ //if, from character at given angle and distance, it DOESNT collide with obstacleMask
                    //if distance is closer
                    if(distanceToTarget < minDistanceToTarget) {
                        minDistanceToTarget = distanceToTarget;
                        chosenTarget = col.GetComponent<CharacterHandler>();
                    }
                }
            }
        }             

        return chosenTarget;
    }


    //upon contact with le weapon, this handles the appropriate response (such as tackign damage, stamina drain, counters etc)
    public virtual void AttackResponse(float damage, CharacterHandler attacker) { 
        string result = "null";//for debug

        // try {
        //     if ((this as AIHandler).GlobalState == GlobalState.DEAD) {
        //         Debug.Log("hes already dead don't bother");
        //         return;
        //     }
        // } catch {}

        if(this.genericState is MoveState) { //no sword drawn and get hit, (todo - do a different stagger)
            TakeDamage(damage, false, attacker); 
            result = "cunt dont have sword out ";

        }

        else if(this.genericState is AttackState) { //TODO AM IN RANGE
            //i am currently in an unblockable attack while being attacked
            //if enemy is simultaneously in attack
            if(attacker.genericState is AttackState){
                //if my attack is unblockable
                if(!(this.genericState as AttackState).chosenMove.blockableAttack){
                    //take damage but dont stagger
                    TakeDamage(damage, false, attacker);
                    result = "both take damage, but reacter staggers only due to unblockable attack";
                } else {
                    //take damage, stagger as usual
                    TakeDamage(damage, true, attacker);
                    result = "both take damage and stagger";
                }
            }
            
        } else if (this.genericState is BlockState){ //todo CHECK IF VALID BLOCK
            //i am blocking an unblockable attack AND if its a valid block (should be null if no target)
            if(!(attacker.genericState as AttackState).chosenMove.blockableAttack) {
                //take damage and stagger
                result = "requester beats block with unblockable, receiver takes damage and staggers";
                    TakeDamage(damage, true, attacker);
           
            } else {
                //drain stamina instead
                DealStamina(damage);
                Array.Find(attacker.audioData, AudioData => AudioData.name == "clang").Play(attacker.AudioSource);
                result = "receiver blocks, only stamina drain";
            }


        } else if (this.genericState is CounterState) { //todo CHECK IF VALID BLOCk
            //if i am countering an unblockable attack
            if(!(attacker.genericState as AttackState).chosenMove.blockableAttack){
                //no damage, but enemy isnt staggared
                //either a heavy attack with long endlag,
                //OR can be instantly followed up with another swing maybe

                result = "requester used unblockable attack but is countered, no effect to either";
            } else {
                result = "receiver counters, requester staggers";
                //proper counter here
            }
            Array.Find(attacker.audioData, AudioData => AudioData.name == "clang").Play(attacker.AudioSource);

            attacker.SetStateDriver(new StaggerState(attacker, attacker.animator));

        } else if (this.genericState is DodgeState) {
            result = "receiver dodged, no damage, stamina only";
            DealStamina(5f);
        } else if (this.genericState is StaggerState) {
            result = "receiver hit when staggered";
            TakeDamage(damage, false, attacker);
            //everytime this is triggered, increment todo
            //"prevent camping when down" counter maybe
        } else { 
            result = "default situation, receiver takes damage and staggers, possible out of range";
            //take damage, stagger
            TakeDamage(damage, true, attacker);

        }

        Debug.Log("REQUESTER: " + attacker.genericState.ToString() 
                + ", REACTER: " + genericState.ToString()
                + ", RESULT: " + result);


    }

    //upon taking damage
    protected virtual void TakeDamage(float damage, bool isStaggerable, CharacterHandler attacker){ 
        Health -= damage;
        heathbar.fillAmount = Health / characterdata.maxHealth;

        Array.Find(attacker.audioData, AudioData => AudioData.name == "flesh").Play(attacker.AudioSource);


        if (isStaggerable && Health > 0) { SetStateDriver(new StaggerState(this, animator)); }
        //if dead:
            //change CombatState to death
                //which in itself handels death stuff 
                    //(including ragdolls, animations, enum)
            //if AI (when overwritten), change AIstate 

    }

    //stamina management
    private IEnumerator staminaRegenCoroutine; //for the actual regening
    private IEnumerator staminaDrainAndCooldown; //for the drain, and short break before allowing cooldown

    //take stamina drain, stop and start appropriate coroutines
    public void DealStamina(float staminaDrain) {
        //cancel the wait from current dealing of stamina
        if(staminaDrainAndCooldown != null) StopCoroutine(staminaDrainAndCooldown);
        //also stop regenerating stamina
        if(staminaRegenCoroutine != null) StopCoroutine(staminaRegenCoroutine);
        
        //deal stamina damage again, restart cooldown
        staminaDrainAndCooldown = TakeStaminaDrain(staminaDrain);
        StartCoroutine(staminaDrainAndCooldown);
    }

    protected IEnumerator StaminaRegeneration() {
        //Debug.Log("in stam regen");
        while (Stamina < characterdata.maxStamina) {
            Stamina += characterdata.staminaRegenerationRatePerS / 10;
            staminabar.fillAmount = Stamina / characterdata.maxStamina; //update el bar
            yield return new WaitForSeconds(0.1f);
        }
        staminaRegenCoroutine = null;
    }

    protected IEnumerator TakeStaminaDrain(float staminaDrain){
        Stamina -= staminaDrain;
        staminabar.fillAmount = Stamina / characterdata.maxStamina;
        yield return new WaitForSeconds(staminaRegenerationWindow);

        //start regening again
        staminaRegenCoroutine = StaminaRegeneration();
        StartCoroutine(staminaRegenCoroutine); 
        staminaDrainAndCooldown = null; //allow reuse   
    }

    #endregion

    #region COMBATFSM
    //everytime the state is changed, do an exit routine (if applicable), switch the state, then trigger start routine (if applicable)
    public void SetStateDriver(GenericState state) { 
        StartCoroutine(SetState(state));
    }

    public IEnumerator SetState(GenericState state) {
        if(genericState != null) yield return StartCoroutine(genericState.OnStateExit());
        genericState = state;
        yield return StartCoroutine(genericState.OnStateEnter());
    }
    #endregion

    #region useful helper functions

    protected Vector3 oldPos;
    protected Vector3 movedPos;
    protected Vector3 lastPos; //math stuff
    protected void CalculateVelocity(){
        oldPos = movedPos;
        movedPos = Vector3.Slerp(oldPos, transform.position - lastPos, .1f); 
        lastPos = transform.position; 
        velocity = movedPos / Time.fixedTime;      
    }

    public IEnumerator layerWeightRoutine;

    public IEnumerator LayerWeightDriver(int layeri, float startVal, float endVal, float smoothness){
        layerWeightRoutine = LayerWeightShift(layeri, startVal, endVal, smoothness);
        yield return StartCoroutine(layerWeightRoutine);
    }

    private IEnumerator LayerWeightShift(int layeri, float startVal, float endVal, float smoothness){
        float temp = startVal;
        while(! (Mathf.Abs(temp - endVal) < 0.01f)) {
               // Debug.Log(temp);

            temp = Mathf.Lerp(startVal, endVal, smoothness);
            startVal = temp;
            animator.SetLayerWeight(layeri, temp);
            yield return null;
        }

        animator.SetLayerWeight(layeri, endVal);
        layerWeightRoutine = null;
    }

    #endregion

    #region foliage
    IEnumerator GrassHandle(){
        while (true){
            //Collider[] foliageInView = Physics.OverlapSphere(transform.position, radius+2f, foliageMask);
            if(Physics.OverlapSphere(transform.position, 4, foliageMask).Length > 0) {
                foreach(Material mat in materials) {
                    mat.SetVector(Shader.PropertyToID("characterPositions"), new Vector2(transform.position.x, transform.position.z));
                   // Debug.Log(mat.name);
                }
                Shader.SetGlobalFloat(Shader.PropertyToID("characterCount"), 10); //temp idk
            } 
            //Debug.Log(foliageInView.Length);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }
    #endregion
}
