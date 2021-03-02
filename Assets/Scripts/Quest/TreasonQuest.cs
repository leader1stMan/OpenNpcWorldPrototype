using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasonQuest : Quest
{
    public SentenceGoal goal1;
    public SentenceGoal redGoal;
    public SentenceGoal pinkGoal;
    public SentenceGoal greenGoal;
    public SentenceGoal questionEndGoal;
    public List<Sentence> trackedSentences1;
    public List<Sentence> redSentences;
    public List<Sentence> pinkSentences;
    public List<Sentence> greenSentences;
    public Sentence goAwaySentence;
    public Sentence againstKingSentence;
    public Sentence questionCompleteQuestion;
    private DialogueManager dialogue;
    public Sentence redEnding;
    public Sentence orangeEnding;
    public Sentence kingEnding;
    public DayAndNightControl control;
    private int startDay;
    private bool firstPartCompleted = false;
    private int redQuestions = 0;
    private int pinkQuestions = 0;
    private int greenQuestions = 0;
    public NPC Theodore;
    private DialogueManager TheodoreDialogue;
    public Sentence TheodoreStartSentence;
    // Start is called before the first frame update
    void Awake()
    {
        goal1 = new SentenceGoal(trackedSentences1);
        goal1.AddHandler(CompleteFirstGoal);
        dialogue = GetComponent<DialogueManager>();

        redGoal = new SentenceGoal(redSentences);
        redGoal.AddHandler(RedSentenceCalled);

        pinkGoal = new SentenceGoal(pinkSentences);
        pinkGoal.AddHandler(PinkSentenceCalled);

        greenGoal = new SentenceGoal(greenSentences);
        greenGoal.AddHandler(GreenSentenceCalled);

        questionEndGoal = new SentenceGoal(questionCompleteQuestion);
        questionEndGoal.AddHandler(CompleteQuestionPart);
    }

    void Update()
    {
        if (firstPartCompleted && startDay + 1 <= control.currentDay)
        {
            if (goal1.selectedSentence.questParameter == "red")
            {
                dialogue.sentence1 = redEnding;
                firstPartCompleted = false;
            }
            if (goal1.selectedSentence.questParameter == "orange")
            {
                dialogue.sentence1 = orangeEnding;
                firstPartCompleted = false;
            }
        }
    }
    void CompleteFirstGoal()
    {
        startDay = control.currentDay;
        firstPartCompleted = true;
        goal1.RemoveHandler(CompleteFirstGoal);
        dialogue.sentence1 = goAwaySentence;
    }

    void GreenSentenceCalled()
    {
        greenQuestions++;
    }

    void PinkSentenceCalled()
    {
        pinkQuestions++;
    }

    void RedSentenceCalled()
    {
        redQuestions++;
    }

    void CompleteQuestionPart()
    {
        int result = greenQuestions - redQuestions;
        if (result < 0)
        {
            AgainstEnding();
        }
        if (result == 0)
        {
            NeutralEnding();
        }
        if (result > 0)
        {
            KingEnding();
        }
    }

    void KingEnding()
    {
        dialogue.sentence1 = kingEnding;
    }

    void NeutralEnding()
    {

    }

    void AgainstEnding()
    {
        dialogue.sentence1 = againstKingSentence;
        TheodoreDialogue = Theodore.GetComponent<DialogueManager>();
        TheodoreDialogue.sentence1 = TheodoreStartSentence;
    }
}
