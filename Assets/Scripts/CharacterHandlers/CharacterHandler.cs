using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

//[System.Serializable] public class CounterEvent : UnityEvent<>{}


public class CharacterHandler : MonoBehaviour
{
    [Header("Core Components/SOs")]
    public CharacterData characterdata;
    public WeaponData weapon; //Todo: list to choose between
    protected Animator animator;
    public MeleeRaycastHandler MeleeRaycastHandler {get; protected set;}

    [Header("Core Members")]
    public Image heathbar;
    public Image staminabar;
    public TextMeshProUGUI debugState; 
    public float staminaRegenerationWindow = 3f;
    public UnityEvent counterEvent;
    public Dictionary<string, int> AnimationHashes { get; private set; }

    //private stuff
    public CombatState combatState {get; private set;}
    public float Health {get; set; }
    public float Stamina {get; set; }
    public Dictionary<string, MeleeMove> MeleeAttacks {get; private set;} //for easier access 
    public MeleeMove MeleeBlock {get; private set; }

    #region Callbacks
    protected virtual void Start() {
        animator = this.GetComponent<Animator>();
        MeleeRaycastHandler = this.GetComponent<MeleeRaycastHandler>();
        Health = characterdata.maxHealth;
        Stamina = characterdata.maxStamina;
        PopulateMeleeMoves();
        setupAnimationHashes();
        SetStateDriver(new DefaultState(this, animator, MeleeRaycastHandler)); //start as default
        
    }
    private void setupAnimationHashes() {
        AnimationHashes = new Dictionary<string, int>();
        AnimationHashes.Add("IsPatrol", Animator.StringToHash("IsPatrol"));
        AnimationHashes.Add("IsAggroWalk", Animator.StringToHash("IsAggroWalk"));
        AnimationHashes.Add("IsSearching", Animator.StringToHash("IsSearching"));
        AnimationHashes.Add("IsStaring", Animator.StringToHash("IsStaring"));
        AnimationHashes.Add("IsAttacking", Animator.StringToHash("IsAttacking"));
        AnimationHashes.Add("IsAgro", Animator.StringToHash("IsAgro"));
        AnimationHashes.Add("IsBlocking", Animator.StringToHash("IsBlocking"));
        AnimationHashes.Add("IsCountering", Animator.StringToHash("IsCountering"));
    }
    private void PopulateMeleeMoves() {
        MeleeAttacks = new Dictionary<string, MeleeMove>();
        foreach(MeleeMove attack in weapon.Attacks) {
            MeleeAttacks.Add(attack.name, attack);
        }
    }

    protected virtual void Update(){
        debugState.SetText(combatState.toString());
    }
    #endregion

    #region core/var manipulation
    //upon contact with le weapon, this handles the appropriate response (such as tackign damage, stamina drain, counters etc)
    public virtual void AttackResponse(float damage, CharacterHandler attackingCharacter) { 
        string result = "null";//for debug

        if(this.combatState is AttackState) { //TODO AM IN RANGE
            //i am currently in an unblockable attack while being attacked
            //if enemy is simultaneously in attack
            if(attackingCharacter.combatState is AttackState){
                //if my attack is unblockable
                if(!(this.combatState as AttackState).chosenMove.blockableAttack){
                    //take damage but dont stagger
                    result = "both take damage, but reacter staggers only due to unblockable attack";
                } else {
                    //take damage, stagger as usual
                    result = "both take damage and stagger";
                }
            }
            
        } else if (this.combatState is BlockState && MeleeRaycastHandler.chosenTarget != null) {
            //i am blocking an unblockable attack AND if its a valid block (should be null if no target)
            if(!(attackingCharacter.combatState as AttackState).chosenMove.blockableAttack) {
                //take damage and stagger
                result = "requester beats block with unblockable, receiver takes damage and staggers";
            } else {
                //drain stamina instead
                result = "receiver blocks, only stamina drain";
            }


        } else if (this.combatState is CounterState) {
            //if i am countering an unblockable attack
            if(!(attackingCharacter.combatState as AttackState).chosenMove.blockableAttack){
                //no damage, but enemy isnt staggared
                //either a heavy attack with long endlag,
                //OR can be instantly followed up with another swing maybe
                result = "requester used unblockable attack but is countered, no effect to either";
            } else {
                result = "receiver counters, requester staggers";
                //proper counter here
            }

        } else if (this.combatState is DodgeState) {
            result = "receiver dodged, no damage, stamina only";
        } else if (this.combatState is StaggerState) {
            result = "receiver hit when staggered";
            //everytime this is triggered, increment
            //"prevent camping when down" counter maybe
        } else { 
            result = "default situation, receiver takes damage and staggers";
            //take damage, stagger
        }

        Debug.Log("REQUESTER: " + attackingCharacter.combatState.toString() 
                + ", REACTER: " + combatState.toString()
                + ", RESULT: " + result);

        DealStamina(damage);

    }

    //ONLY method that allows damage
    protected virtual void TakeDamage(float damage){ 
        Health -= damage;
        heathbar.fillAmount = Health / characterdata.maxHealth;
    
        //if dead:
            //change CombatState to death
                //which in itself handels death stuff 
                    //(including ragdolls, animations, enum)
            //if AI (when overwritten), change AIstate 

    }

    private IEnumerator staminaRegenCoroutine;
    private IEnumerator staminaDrainAndCooldown;

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
        Debug.Log("in stam regen");
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
    public void SetStateDriver(CombatState state) { 
        StartCoroutine(SetState(state));
    }

    private IEnumerator SetState(CombatState state) {
        if(combatState != null) yield return StartCoroutine(combatState.OnStateExit());
        combatState = state;
        yield return StartCoroutine(combatState.OnStateEnter());
    }
    #endregion

}
