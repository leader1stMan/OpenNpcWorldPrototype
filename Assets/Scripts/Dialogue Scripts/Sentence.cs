using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sentence.asset", menuName = "Sentence")]
public class Sentence : ScriptableObject
{
    public string text;
    public string answer;

    public SentenceGoal goal;
    public Sentence nextSentence;
    public List<Sentence> choices;

    public string questParameter;

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
