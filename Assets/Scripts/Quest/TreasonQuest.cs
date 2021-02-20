using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasonQuest : Quest
{
    public SentenceGoal goal1;
    public List<Sentence> trackedSentences1;
    // Start is called before the first frame update
    void Awake()
    {
        goal1 = new SentenceGoal(trackedSentences1);
        goal1.AddHandler(CompleteFirstGoal);
    }

    void CompleteFirstGoal()
    {
        Debug.Log(goal1.selectedSentence.text);
    }
}
