using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Dialogue : MonoBehaviour
{
    private Queue<string> sentences;

    public Text dialogueText;
    public Text nameText;
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue (dialoguedictionary dialogue){
        nameText.text = dialogue.name;
        sentences.Clear();
        foreach(string sentence in dialogue.sentences){
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }
    public void DisplayNextSentence(){
        if(sentences.Count ==0){
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
    }
    void EndDialogue(){

    }
}
