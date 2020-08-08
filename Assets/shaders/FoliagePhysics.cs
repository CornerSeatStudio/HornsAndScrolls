using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class FoliagePhysics : MonoBehaviour
{
    private LayerMask foliageMask;
    public Material[] materials;
    private float foliageRadius;
    void Start() {
        foliageMask = LayerMask.GetMask("Foliage");
        Renderer rend = GameObject.FindGameObjectsWithTag("Foliage").First().GetComponent<Renderer>();
        //todo: all foliage should be the same size
        foliageRadius = Mathf.Max(rend.bounds.max.x, rend.bounds.max.z) - (rend.bounds.center.x + rend.bounds.center.z)/2;
        Debug.Log(foliageRadius);
        StartCoroutine(GrassHandle());
    }

    IEnumerator GrassHandle(){
        while (true){
            //Collider[] foliageInView = Physics.OverlapSphere(transform.position, radius+2f, foliageMask);
            if(Physics.OverlapSphere(transform.position, foliageRadius/2, foliageMask).Length > 0) {
                foreach(Material mat in materials) {
                    mat.SetVector("characterPositions", new Vector2(transform.position.x, transform.position.z));
                    Debug.Log(mat.name);
                }
                Shader.SetGlobalFloat("characterCount", 10); //temp idk
            } 
            //Debug.Log(foliageInView.Length);
            yield return null;
        }
    }


}


