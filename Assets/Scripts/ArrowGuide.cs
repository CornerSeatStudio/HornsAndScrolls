using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowGuide : MonoBehaviour
{

     [SerializeField] private LineRenderer lr;
     public float maxTrace = 20;

    // Start is called before the first frame update
    void Start()
    {
        lr = this.GetComponent<LineRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        //deal with collisions
        RaycastHit hit;

        if(Physics.Raycast(transform.position, transform.forward, out hit)){
            if(hit.collider){
                lr.SetPosition(1, new Vector3(0, 0, hit.distance));
            } 

        } else {
                lr.SetPosition(1, new Vector3(0, 0, maxTrace));
            
        }
     
        
    }
}
