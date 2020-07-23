using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour
{
    // Start is called before the first frame update
    private bool hasObject;

    private void Start()
    {




    }   

    private void OnTriggerEnter(Collider col)
    {
        hasObject=true;
    }
}
