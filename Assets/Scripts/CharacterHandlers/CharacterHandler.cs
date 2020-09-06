using System.Collections;
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
    public Collider weaponHitbox;
    public InventoryObject inventory;

    [Header("Debug Members")]
    public Image heathbar;
    public Image staminabar;
    public TextMeshProUGUI debugState; 
    
    [Header ("Foliage Handling")]
    public Material[] materials;


    //private stuff
    protected Animator animator;
    public AudioSource AudioSource {get; private set;}
    public Dictionary<string, MeleeMove> MeleeAttacks {get; private set;} 
    public MeleeMove MeleeBlock {get; private set; }
    public float Health {get; private set; }
    public float Stamina {get; private set; }
    public GenericState genericState {get; protected set;}
    private LayerMask foliageMask;
    public bool CanAttack {get; set;} = false; //checks if weapon is in contact with target (via weapon script on weapon)
    public GameObject AttackReceiver {get; set; }
    public TrailRenderer WeaponTrail {get; private set;}
    

    #region Callbacks
    protected virtual void Start() {
        animator = this.GetComponent<Animator>();        
        AudioSource = this.GetComponent<AudioSource>();
        Health = characterdata.maxHealth;
        Stamina = characterdata.maxStamina;

        
        DisableRagdoll(); //NECCESARY to a. disable ragdoll and b. not fuck up attack script
        PopulateMeleeMoves();

        //foliage stuff
        foliageMask = LayerMask.GetMask("Foliage");
        StartCoroutine(GrassHandle()); 
        
        //weapon trail
        WeaponTrail = GetComponentInChildren<TrailRenderer>();
        WeaponTrail.enabled = false;

    }

    protected virtual void Update(){
        if(genericState != null) debugState.SetText(genericState.ToString());
        // if(Input.GetKeyDown(KeyCode.P)){
        //     foreach(InventorySlot i in inventory.items){
        //         Debug.Log($"{i.item.name} : {i.quanity}");
        //     }
        // }
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
            if (col.gameObject != this.gameObject && col != weaponHitbox) {
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

    #region Combat Core

    //upon contact with le weapon, this handles the appropriate response (such as tackign damage, stamina drain, counters etc)
    public virtual void AttackRequest(float damage){
        CharacterHandler receiver = AttackReceiver.GetComponent<CharacterHandler>();
        
        string result = "null";//for debug

        // try {
        //     if ((this as AIHandler).GlobalState == GlobalState.DEAD) {
        //         Debug.Log("hes already dead don't bother");
        //         return;
        //     }
        // } catch {}

        

        if(receiver.genericState is MoveState) { //target has no sword drawn and get hit, (todo - do a different stagger)
            DealDamageAndCheckDeathDataManagement(damage, false, receiver); 
            result = "cunt dont have sword out ";
        }

        else if(receiver.genericState is AttackState) { //if they are attacking
            //but i am also simultaneously in attack
            if(this.genericState is AttackState){
                //if their attack is unblockable
                if(!(receiver.genericState as AttackState).chosenMove.blockableAttack){
                    //take damage but dont stagger
                    DealDamageAndCheckDeathDataManagement(damage, false, receiver);
                    result = "both take damage, but blockbale buboons stagger only due to unblockable attack";
                } else {
                    //take damage, stagger as usual
                    DealDamageAndCheckDeathDataManagement(damage, true, receiver);
                    result = "both take damage and stagger";
                }
            }
            
        } else if (receiver.genericState is BlockState){ //if they are blocking
            //and if i am using an unblockable attack
            if(!(this.genericState as AttackState).chosenMove.blockableAttack) {
                //take damage and stagger
                result = "requester beats block with unblockable, receiver takes damage and staggers";
                DealDamageAndCheckDeathDataManagement(damage, true, receiver);
           
            } else {
                //drain stamina instead
                receiver.DealStamina(damage);
                Array.Find(this.audioData, AudioData => AudioData.name == "clang").Play(this.AudioSource);
                result = "receiver blocks, only stamina drain";
            }


        } else if (receiver.genericState is CounterState) { //if they're countering
            //if they are countering an unblockable attack
            if(!(this.genericState as AttackState).chosenMove.blockableAttack){
                //no damage, but enemy isnt staggared
                //either a heavy attack with long endlag,
                //OR can be instantly followed up with another swing maybe

                result = "requester used unblockable attack but is countered, no effect to either";
            } else {
                result = "receiver counters, requester staggers";
                //proper counter here + stamina drain
            }
            Array.Find(this.audioData, AudioData => AudioData.name == "clang").Play(this.AudioSource);

            this.SetStateDriver(new StaggerState(this, this.animator));

        } else if (receiver.genericState is DodgeState) { //if they're dodging
            result = "receiver dodged, no damage, stamina only";
            receiver.DealStamina(5f);
        } else if (receiver.genericState is StaggerState) {
            result = "receiver hit when staggered";
            DealDamageAndCheckDeathDataManagement(damage, false, receiver);
            //everytime this is triggered, increment todo
            //"prevent camping when down" counter maybe
        } else { 
            result = "default situation, receiver takes damage and staggers, possible out of range";
            //take damage, stagger
            DealDamageAndCheckDeathDataManagement(damage, true, receiver);

        }


        Debug.Log("REQUESTER: " + this.genericState.ToString() 
                + ", REACTER: " + receiver.genericState.ToString()
                + ", RESULT: " + result);

        //clear out fields
        CanAttack = false;
        AttackReceiver = null;

    }


    //upon taking damage
    public void GainHealth(float healthGain) => Health = Mathf.Min(characterdata.maxHealth, Health + healthGain);
        
    protected void DealDamageAndCheckDeathDataManagement(float damage, bool isStaggerable, CharacterHandler receiver){
        if(receiver.TakeDamageAndCheckDeath(damage, isStaggerable, this)) {
            Debug.Log("death management goes here");
        }
    }

    protected virtual bool TakeDamageAndCheckDeath(float damage, bool isStaggerable, CharacterHandler attacker){ 
        Health -= damage;
        heathbar.fillAmount = Health / characterdata.maxHealth;

        Array.Find(attacker.audioData, AudioData => AudioData.name == "flesh").Play(attacker.AudioSource);


        if (isStaggerable && Health > 0) { 
            SetStateDriver(new StaggerState(this, animator)); 
        }

        return Health <= 0 ? true : false;
        //if dead:
            //change CombatState to death
                //which in itself handels death stuff 
                    //(including ragdolls, animations, enum)
            //if AI (when overwritten), change AIstate 

    }

    //stamina management
    public void GainStamina(float staminaGain) => Stamina = Mathf.Min(characterdata.maxStamina, Stamina + staminaGain);
        
    

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
        StartCoroutine(staminaDrainAndCooldown); //TODO stop at death
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
        yield return new WaitForSeconds(characterdata.staminaRegenerationWindow);

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
            yield return new WaitForFixedUpdate();
        }
    }
    #endregion
}
