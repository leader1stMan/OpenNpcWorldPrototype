using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SentenceGoal
{
    public List<Sentence> trackedSentences;
    public Sentence selectedSentence = null;

    public delegate void GoalCompletedHandler();
    GoalCompletedHandler goalHandlers;

    public SentenceGoal()
    { }

    public SentenceGoal(Sentence sentence)
    {
        trackedSentences = new List<Sentence>();
        trackedSentences.Add(sentence);
        sentence.goal = this;
    }
    public SentenceGoal(List<Sentence> sentences)
    {
        trackedSentences = sentences;
        foreach (Sentence s in sentences)
        {
            s.goal = this;
        }
    }

    public void completeSentence(Sentence sentence)
    {
        selectedSentence = sentence;
        goalHandlers?.Invoke();
    }

    public void AddHandler(GoalCompletedHandler a)
    {
        goalHandlers += a;
    }

    public void RemoveHandler(GoalCompletedHandler a)
    {
        if (goalHandlers == null) return;
        goalHandlers -= a;
    }

    public void AddSentenceToTrack(Sentence sentence)
    {
        trackedSentences.Add(sentence);
    }

    public void AddSentenceToTrack(List<Sentence> sentences)
    {
        int index = 0;
        foreach (Sentence s in sentences)
        {
            trackedSentences.Add(sentences[index]);
            index++;
        }
    }
}
