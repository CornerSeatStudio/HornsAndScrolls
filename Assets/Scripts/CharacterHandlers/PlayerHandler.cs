using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandler : CharacterHandler
{

    public List<WeaponData> weapons;
    public float startupDelay = .2f;
    public float endDelay = .2f; 
    public float damage = 3f;
    private bool attackCooldown = false;
    private bool IsAttacking;
    
    public enum PlayerState { HIDDEN, COMBAT, DEAD };

    
    void Update() {

        if(Input.GetButtonDown("Fire1") == true) {
            if(!attackCooldown){
                Debug.Log("Fire1'd");
                StartCoroutine(HitDetection.InitAttack(startupDelay, endDelay, damage));
                StartCoroutine(CooldownManager());
                
            }
            IsAttacking=true;
        }else{
            IsAttacking=false;
        }
        
    }

    private IEnumerator CooldownManager() { //prevents spam clicking
        attackCooldown = true;
        yield return new WaitForSeconds(startupDelay + endDelay);
        attackCooldown = false;
    }

    void LateUpdate(){
        animator.SetBool("IsAttacking", IsAttacking);
    }
  
    

}
