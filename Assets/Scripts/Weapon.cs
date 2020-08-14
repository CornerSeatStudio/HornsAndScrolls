using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private CharacterHandler weaponUser;

    public void Start(){
        try {
            weaponUser = this.transform.root.GetComponent<CharacterHandler>();
        } catch {
            Debug.LogWarning("WEAPON NEEDS CHARACTERHANDLER");
        }
    }
    public virtual void OnTriggerEnter(Collider col){
        if(col.gameObject != weaponUser.gameObject && (col.CompareTag("Enemy") || col.CompareTag("Player"))) {
            weaponUser.CanAttack = true;
            weaponUser.AttackReceiver = col.gameObject;
           // Debug.Log("weapon entered " + col.name);
        }
    }
    public virtual void OnTriggerExit(Collider col){
        if(col.gameObject != weaponUser.gameObject && (col.CompareTag("Enemy") || col.CompareTag("Player"))) {
            weaponUser.CanAttack = false;
            weaponUser.AttackReceiver = null;
          //  Debug.Log("weapon exitited " + col.name);
        }
        
    }
}
