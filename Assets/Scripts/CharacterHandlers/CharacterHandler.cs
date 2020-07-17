using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class CharacterHandler : MonoBehaviour {

    [Header("Core Components/SOs")]
    public CharacterData characterdata;
    public WeaponData weapon; 

    [Header("Core Members")]
    public Image heathbar;
    public Image staminabar;
    public TextMeshProUGUI debugState; 
    public float staminaRegenerationWindow = 3f;

    //private stuff
    protected Animator animator;

    public Dictionary<string, int> AnimationHashes { get; private set; }
    public Dictionary<string, MeleeMove> MeleeAttacks {get; private set;} 
    public MeleeMove MeleeBlock {get; private set; }
    public float Health {get; private set; }
    public float Stamina {get; private set; }
    public GenericState genericState {get; private set;}

    #region Callbacks
    protected virtual void Start() {
        
        animator = this.GetComponent<Animator>();        
        
        Health = characterdata.maxHealth;
        Stamina = characterdata.maxStamina;

        
        DisableRagdoll(); //NECCESARY to a. disable ragdoll and b. not fuck up attack script
        setupAnimationHashes();
        PopulateMeleeMoves(); 
        
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

    //put animations as hashes for efficiency
    protected virtual void setupAnimationHashes() {
        AnimationHashes = new Dictionary<string, int>();
        AnimationHashes.Add("IsPatrol", Animator.StringToHash("IsPatrol"));
        AnimationHashes.Add("IsAggroWalk", Animator.StringToHash("IsAggroWalk"));
        AnimationHashes.Add("IsSearching", Animator.StringToHash("IsSearching"));
        AnimationHashes.Add("IsStaring", Animator.StringToHash("IsStaring"));
        AnimationHashes.Add("IsAggro", Animator.StringToHash("IsAgro"));

        AnimationHashes.Add("IsAttacking", Animator.StringToHash("IsAttacking"));
        AnimationHashes.Add("IsBlocking", Animator.StringToHash("IsBlocking"));
        AnimationHashes.Add("IsCountering", Animator.StringToHash("IsCountering"));
    
        AnimationHashes.Add("Idle", Animator.StringToHash("Idle"));
        AnimationHashes.Add("Jogging", Animator.StringToHash("Jogging"));
        AnimationHashes.Add("Sprinting", Animator.StringToHash("Sprinting"));
        AnimationHashes.Add("Walking", Animator.StringToHash("Walking"));
        AnimationHashes.Add("NormalMoveBlend", Animator.StringToHash("NormalMoveBlend"));
        AnimationHashes.Add("Crouching", Animator.StringToHash("Crouching"));
        AnimationHashes.Add("CrouchWalking", Animator.StringToHash("CrouchWalking"));
        AnimationHashes.Add("WeaponOut", Animator.StringToHash("WeaponOut"));
        AnimationHashes.Add("XCombatMove", Animator.StringToHash("XCombatMove"));
        AnimationHashes.Add("ZCombatMove", Animator.StringToHash("ZCombatMove"));
        AnimationHashes.Add("CombatWalking", Animator.StringToHash("CombatWalking"));
        AnimationHashes.Add("Dodging", Animator.StringToHash("Dodging"));

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
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public CharacterHandler FindTarget(MeleeMove meleeMove){
        float minDistanceToTarget = float.MaxValue; //guarantees first check in findingInteractableTargets
        CharacterHandler chosenTarget = null; //reset character chosen
        //cast a sphere over player, store everything inside col
        Collider[] targetsInView = Physics.OverlapSphere(transform.position, meleeMove.range, targetMask);
        
        if(targetsInView.Length == 0) {Debug.Log("nothing found in find target function"); return null; }

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
    public virtual void AttackResponse(float damage, CharacterHandler attackingCharacter) { 
        string result = "null";//for debug

        if(this.genericState is AttackState) { //TODO AM IN RANGE
            //i am currently in an unblockable attack while being attacked
            //if enemy is simultaneously in attack
            if(attackingCharacter.genericState is AttackState){
                //if my attack is unblockable
                if(!(this.genericState as AttackState).chosenMove.blockableAttack){
                    //take damage but dont stagger
                    TakeDamage(damage);
                    result = "both take damage, but reacter staggers only due to unblockable attack";
                } else {
                    //take damage, stagger as usual
                    TakeDamage(damage);
                    SetStateDriver(new StaggerState(this, animator));
                    result = "both take damage and stagger";
                }
            }
            
        } else if (this.genericState is BlockState ){ //todo && MeleeRaycastHandler.chosenTarget != null) {
            //i am blocking an unblockable attack AND if its a valid block (should be null if no target)
            if(!(attackingCharacter.genericState as AttackState).chosenMove.blockableAttack) {
                //take damage and stagger
                result = "requester beats block with unblockable, receiver takes damage and staggers";
                    TakeDamage(damage);
                    SetStateDriver(new StaggerState(this, animator));
           
            } else {
                //drain stamina instead
                TakeStaminaDrain(damage);
                result = "receiver blocks, only stamina drain";
            }


        } else if (this.genericState is CounterState) {
            //if i am countering an unblockable attack
            if(!(attackingCharacter.genericState as AttackState).chosenMove.blockableAttack){
                //no damage, but enemy isnt staggared
                //either a heavy attack with long endlag,
                //OR can be instantly followed up with another swing maybe
                result = "requester used unblockable attack but is countered, no effect to either";
            } else {
                result = "receiver counters, requester staggers";
                attackingCharacter.SetStateDriver(new StaggerState(attackingCharacter, attackingCharacter.animator));
                //proper counter here
            }

        } else if (this.genericState is DodgeState) {
            result = "receiver dodged, no damage, stamina only";
            TakeStaminaDrain(3f);
        } else if (this.genericState is StaggerState) {
            result = "receiver hit when staggered";
            TakeDamage(damage);
            //everytime this is triggered, increment todo
            //"prevent camping when down" counter maybe
        } else { 
            result = "default situation, receiver takes damage and staggers, possible out of range";
            //take damage, stagger
            TakeDamage(damage);
            SetStateDriver(new StaggerState(this, animator));

        }

        Debug.Log("REQUESTER: " + attackingCharacter.genericState.ToString() 
                + ", REACTER: " + genericState.ToString()
                + ", RESULT: " + result);


    }

    //upon taking damage
    protected virtual void TakeDamage(float damage){ 
        Health -= damage;
        heathbar.fillAmount = Health / characterdata.maxHealth;
    
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

    protected IEnumerator SetState(GenericState state) {
        if(genericState != null) yield return StartCoroutine(genericState.OnStateExit());
        genericState = state;
        yield return StartCoroutine(genericState.OnStateEnter());
    }
    #endregion


}
