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

    void LateUpdate(){
        debugState.SetText(combatState.toString());
    }

    #endregion

    #region core/var manipulation
    //upon contact with le weapon, this handles the appropriate response (such as tackign damage, stamina drain, counters etc)
    public virtual void AttackResponse(float damage, CharacterHandler attackingCharacter) { 
        
        if(this.combatState is AttackState) {
            //i am currently in an unblockable attack while being attacked
            //if enemy is simultaneously in attack
            if(attackingCharacter.combatState is AttackState){
                //if my attack is unblockable
                if(!(this.combatState as AttackState).chosenMove.blockableAttack){
                    //take damage but dont stagger
                } else {
                    //take damage, stagger as usual
                }
            }
            
        } else if (this.combatState is BlockState) {
            //i am blocking an unblockable attack
            if(!(attackingCharacter.combatState as AttackState).chosenMove.blockableAttack) {
                //take damage and stagger
            } else {
                //drain stamina instead
            }


        } else if (this.combatState is CounterState) {
            //if i am countering an unblockable attack
            if(!(attackingCharacter.combatState as AttackState).chosenMove.blockableAttack){
                //no damage, but enemy isnt staggared
                //either a heavy attack with long endlag,
                //OR can be instantly followed up with another swing maybe
            } else {
                //proper counter here
            }

        } else if (this.combatState is DodgeState) {

        } else if (this.combatState is DodgeState) {
        } else {
            //take damage, stagger
        }



        


        TakeDamage(damage);

    }

    public virtual void TakeDamage(float damage){ 
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
