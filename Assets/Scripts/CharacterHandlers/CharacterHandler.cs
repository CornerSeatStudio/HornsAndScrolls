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
    protected MeleeRaycastHandler meleeRaycastHandler;
    public Dictionary<string, MeleeMove> MeleeMoves {get; private set;} //for easier access 


    [Header("Core Members")]
    public Image heathbar;
    public TextMeshProUGUI debugState; 
    public CombatState combatState {get; private set;}
    public float Health {get; set; }
    public float Stamina {get; set; }

    public UnityEvent counterEvent;

    
    #region Callbacks
    protected virtual void Start() {
        animator = this.GetComponent<Animator>();
        meleeRaycastHandler = this.GetComponent<MeleeRaycastHandler>();
        Health = characterdata.maxHealth;
        Stamina = characterdata.maxStamina;
        PopulateMeleeMoves();
        SetStateDriver(new DefaultState(this, animator, meleeRaycastHandler)); //start as default
    }

    private void PopulateMeleeMoves() {
        MeleeMoves = new Dictionary<string, MeleeMove>();
        foreach(MeleeMove attack in weapon.MeleeMoves) {
            MeleeMoves.Add(attack.name, attack);
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

        if(this.combatState is AttackState) {
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
            
        } else if (this.combatState is BlockState) {
            //i am blocking an unblockable attack
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

        TakeDamage(damage);

    }

    protected virtual void TakeDamage(float damage){ 
        Health -= damage;
        heathbar.fillAmount = Health / characterdata.maxHealth;
    }

    protected void TakeStaminaDrain(float staminaDrain){
        Stamina -= staminaDrain;
    }

    

    

    public virtual void Counter() {

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
