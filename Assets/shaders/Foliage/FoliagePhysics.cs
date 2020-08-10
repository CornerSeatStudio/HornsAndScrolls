using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class FoliagePhysics : MonoBehaviour
{
    private LayerMask foliageMask;
    public Material[] materials;
    private float foliageMinRadius;
    void Start() {
        foliageMask = LayerMask.GetMask("Foliage");
        Renderer rend = GameObject.FindGameObjectsWithTag("Foliage").First().GetComponent<Renderer>();
        StartCoroutine(GrassHandle());

        //todo: all foliage should be the same size
        foliageMinRadius = 4;
        //Debug.Log(foliageRadius);
    }

    IEnumerator GrassHandle(){
        while (true){
            //Collider[] foliageInView = Physics.OverlapSphere(transform.position, radius+2f, foliageMask);
            if(Physics.OverlapSphere(transform.position, foliageMinRadius, foliageMask).Length > 0) {
                foreach(Material mat in materials) {
                    mat.SetVector(Shader.PropertyToID("characterPositions"), new Vector2(transform.position.x, transform.position.z));
                   // Debug.Log(mat.name);
                }
                Shader.SetGlobalFloat(Shader.PropertyToID("characterCount"), 10); //temp idk
            } 
            //Debug.Log(foliageInView.Length);
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
    }


}


