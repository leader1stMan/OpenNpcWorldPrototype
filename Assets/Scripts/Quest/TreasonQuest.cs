using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasonQuest : Quest
{
    public SentenceGoal goal1;
    public List<Sentence> trackedSentences1;
    private DialogueManager dialogue;
    public Sentence redEnding;
    public Sentence orangeEnding;
    public DayAndNightControl control;
    private int startDay;
    private bool firstPartCompleted = false;
    // Start is called before the first frame update
    void Awake()
    {
        goal1 = new SentenceGoal(trackedSentences1);
        goal1.AddHandler(CompleteFirstGoal);
        dialogue = GetComponent<DialogueManager>();
    }

    void Update()
    {
        if (firstPartCompleted && startDay + 1 <= control.currentDay)
        {
            if (goal1.selectedSentence.questParameter == "red")
            {
                dialogue.sentence1 = redEnding;
            }
            if (goal1.selectedSentence.questParameter == "orange")
            {
                dialogue.sentence1 = orangeEnding;
            }
        }
    }
    void CompleteFirstGoal()
    {
        startDay = control.currentDay;
        firstPartCompleted = true;
    }
}
