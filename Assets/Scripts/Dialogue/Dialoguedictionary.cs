using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDictionary", menuName = "ScriptableObjects/DialogueDictionary")]
public class Dialoguedictionary : ScriptableObject
{
    public string charName;
    [TextArea(3,10)]
    public string[] sentences;
}
 