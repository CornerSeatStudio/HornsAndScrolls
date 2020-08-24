using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dialoguetrigger : MonoBehaviour
{
    public dialoguedictionary dialogue; 

    public void TriggerDialogue(){
        FindObjectOfType<Dialogue>().StartDialogue(dialogue);    
    }
}
