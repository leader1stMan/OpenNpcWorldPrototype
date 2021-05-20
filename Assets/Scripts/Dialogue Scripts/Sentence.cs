using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sentence.asset", menuName = "Sentence")]
public class Sentence : ScriptableObject
{
    public string text; //What the player says
    public string answer; //Npc's answer to what player says

    public SentenceGoal goal;
    public Sentence nextSentence;
    public List<Sentence> choices; /*Player's choices at what the npc says. These all have their own Sentence class
                                    * Which means senteces can be connected together without code*/

    public string questParameter;
    public bool CallShop;
    public bool agressiveAfter;

    public Sentence(string newText)
    {
        choices = new List<Sentence>();
        text = newText;
    }

    public string GetText() { return text; }

    public bool HasPaths() { return choices.Count > 0; }

    public int GetPaths() { return choices.Count; }

    public Sentence GetSentence(int number) { return choices[number]; }

}
