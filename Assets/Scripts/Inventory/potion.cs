using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class potion : MonoBehaviour
{
    public float potions;
    // Start is called before the first frame update
    void Start()
    {
    if(Input.GetKeyDown(KeyCode.Alpha1)){
        potions=potions-1;
    }
    }

    public void changedscenes(){
        potions=3;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
