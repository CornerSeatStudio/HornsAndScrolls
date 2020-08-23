using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    private Queue<string> sentences;



    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue (dialoguedictionary dialogue){
        Debug.Log("Starting");
    }
}
