using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public CharacterHandler weaponUser;

    public void Start(){
       // weaponUser = this.transform.root.GetComponent<CharacterHandler>();
        //Debug.Log("badsf");
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
