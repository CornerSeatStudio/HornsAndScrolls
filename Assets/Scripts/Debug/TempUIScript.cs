using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempUIScript : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {   
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }
}
