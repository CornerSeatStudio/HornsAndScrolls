using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHandler : MonoBehaviour
{
    [Header("Core Components/SOs")]
    public CharacterData characterdata;
    public WeaponData weapon; //Todo: list to choose between
    protected Animator animator;
    protected MeleeRaycastHandler meleeRaycastHandler;
    public Dictionary<string, MeleeMove> MeleeMoves {get; private set;} //for easier access 


    [Header("Core Members")]
    protected CombatState combatState;
    public float Health {get; set; }
    
    #region Callbacks
    protected virtual void Start() {
        animator = this.GetComponent<Animator>();
        meleeRaycastHandler = this.GetComponent<MeleeRaycastHandler>();
        Health = characterdata.maxHealth;
        PopulateMeleeMoves();

        SetStateDriver(new DefaultState(this, animator, meleeRaycastHandler)); //start as default
    }

    private void PopulateMeleeMoves() {
        MeleeMoves = new Dictionary<string, MeleeMove>();
        foreach(MeleeMove attack in weapon.MeleeMoves) {
            MeleeMoves.Add(attack.name, attack);
        }
    }
    #endregion

    #region core
    public virtual void AttackResponse(float damage, CharacterHandler attackingCharacter) { //invoked every time character receives an attack
        //if the character is blocking or was hit in the middle of the attack
        //additionally, if the player countered or dodged, 
        
        //take LAST PERCIEVED EVENT

        //todo: put in respective classes (replace/add to TakeDamage method)

        //attacking anyone
        //if character is blocking && I am using blockable attack, dont do damage but finish animation - global flag toggle
        //if I was hit && I am using blockable attack, stagger instead

        //attacking Player
        //if i, AI, was hit && i, AI, am using unblockable attack, damage to player as usual
        //if player is blocking && i, AI, am using unblockable attack, deal damage still
        //if player is countering && i, AI, am using unblockable attack, act like blockable attack
        //if player is countering && i, AI, am using blockable attack, begin stagger animation, yield return slower maybe?
        //if player dodges, do no damage/nothing

        //if all else fails, deal the damage
    }

    public virtual void TakeDamage(float damage){ 
        Health -= damage;
    }

    public virtual void Block() { //deal with block (and counter for player)
        

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
