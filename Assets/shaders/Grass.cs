using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    void Start() {
        // Shader.SetGlobalFloat("globalGrassHeight", GetComponent<Collider>().bounds.size.y);
        // Shader.SetGlobalFloat("globalGrassFloor", transform.position.y - (GetComponent<Collider>().bounds.size.y / 2));
        // Debug.Log(transform.position.y - (GetComponent<Collider>().bounds.size.y / 2));
    }

    void Update() {
        //to have player influence grass, get the direction/distance maybe?
       // Shader.SetGlobalFloat3("globalPlayerPos", );
    }
    
}
