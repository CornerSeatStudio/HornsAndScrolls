using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHandler : MonoBehaviour
{
    public CharacterData data;
    private float health;
    public HitDetection hitDetection;

    void Start() {
        hitDetection = this.GetComponent<HitDetection>();
        health = data.maxHealth;
    }

    public void takeDamage(float damage){
        health -= damage;
    }
    

}
