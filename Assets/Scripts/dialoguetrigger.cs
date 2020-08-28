using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Dialoguetrigger : MonoBehaviour
{
    public Dialoguedictionary dialogue; 

    public void TriggerDialogue(){
        FindObjectOfType<Dialogue>().StartDialogue(dialogue);    
    }
    
}
 