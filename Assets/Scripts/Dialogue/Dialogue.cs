using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Dialogue : MonoBehaviour
{ 
    private PlayerHandler player;
    private Queue<string> sentences;
    public Text dialogueText;
    public Animator animator;
    public Animator priestStuff; 
    
    void Start()
    {
        sentences = new Queue<string>();
        player = FindObjectOfType<PlayerHandler>();
    }

    public void DialogueOnTrigger(Dialoguedictionary dialogue){
        StartDialogue(dialogue);

        //lock player out of movement
        player.InDialogue = true;
    }

    public void StartDialogue (Dialoguedictionary dialogue){
        //nameText.text = dialogue.name;
        try{ sentences.Clear(); } catch {}
        animator.SetBool("IsOpen",true);


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

        if(priestStuff != null) { priestStuff.SetTrigger(Animator.StringToHash("Speak")); }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
    }
    void EndDialogue(){
        player.InDialogue = false;
        animator.SetBool("IsOpen",false);
    }
}
