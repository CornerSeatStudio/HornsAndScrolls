using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialoguetrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public dialoguedictionary dialogue; 

    public void TriggerDialogue(){
        FindObjectOfType<Dialogue>().StartDialogue(dialogue);    
    }
}
