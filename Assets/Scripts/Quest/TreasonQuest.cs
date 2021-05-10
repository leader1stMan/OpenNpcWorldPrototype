using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasonQuest : Quest
{
    //Handlers are added to goals and are executed when a goal is complete
    private SentenceGoal goal1;
    private SentenceGoal redGoal;
    private SentenceGoal pinkGoal;
    private SentenceGoal greenGoal;
    private SentenceGoal AlchemistGoal;
    private SentenceGoal questionEndGoal;
    private SentenceGoal FromNeutralToAgainst;
    private SentenceGoal AgainstEndingComplete;

    //List of sentences that adds to a variable and by that changes the path of the dialogue
    public List<Sentence> trackedSentences1;
    public List<Sentence> redSentences;
    public List<Sentence> pinkSentences;
    public List<Sentence> greenSentences;
    public List<Sentence> AlchemistsSentences;
    private List<Sentence> BetweenNeutralAndAgainst;
    public List<Sentence> TheodoreQuestComplete;

    //Variables that are added by each sentences in each List<Sentence>
    private int redQuestions = 0;
    private int pinkQuestions = 0;
    private int greenQuestions = 0;

    //Start sentences of each section
    public Sentence goAwaySentence;
    public Sentence againstKingSentence;
    public Sentence neutralEnding;
    public Sentence questionCompleteQuestion;
    public Sentence GauvainWithPotion;
    public Sentence GauvainWithoutPotion;
    public Sentence redEnding;
    public Sentence orangeEnding;
    public Sentence kingEnding;
    public Sentence TheodoreStartSentence;
    public Sentence AlchemistStartSentence;

    public DayAndNightControl control;
    private DialogueManager dialogue;
    
    public NPC Theodore;
    private DialogueManager TheodoreDialogue;
    public NPC Alchemist;
    private DialogueManager AlchemistDialogue;

    private bool withPotion;

    void Awake()
    {
        goal1 = new SentenceGoal(trackedSentences1);
        goal1.AddHandler(CompleteFirstGoal);
        dialogue = GetComponent<DialogueManager>(); //DialogueManager executes the dialogue system

        redGoal = new SentenceGoal(redSentences);
        redGoal.AddHandler(RedSentenceCalled);

        pinkGoal = new SentenceGoal(pinkSentences);
        pinkGoal.AddHandler(PinkSentenceCalled);

        greenGoal = new SentenceGoal(greenSentences);
        greenGoal.AddHandler(GreenSentenceCalled);

        questionEndGoal = new SentenceGoal(questionCompleteQuestion);
        questionEndGoal.AddHandler(CompleteQuestionPart);

        AlchemistGoal = new SentenceGoal(AlchemistsSentences);
        AlchemistGoal.AddHandler(AlchemistCompleted);

        BetweenNeutralAndAgainst = new List<Sentence>();
        BetweenNeutralAndAgainst.Add(GauvainWithoutPotion);
        BetweenNeutralAndAgainst.Add(GauvainWithPotion);
        FromNeutralToAgainst = new SentenceGoal(BetweenNeutralAndAgainst);
        FromNeutralToAgainst.AddHandler(EnableAgainstFromNeutral);

        AgainstEndingComplete = new SentenceGoal(TheodoreQuestComplete);
        AgainstEndingComplete.AddHandler(AgainstEndingCompleted);
    }

    void CompleteFirstGoal()
    {
        StartCoroutine("Day");
        goal1.RemoveHandler(CompleteFirstGoal);
        dialogue.currentSentence = goAwaySentence;
    }

    IEnumerator Day()
    {
        yield return new WaitForSeconds(control.SecondsInAFullDay);
        if (goal1.selectedSentence.questParameter == "red")
        {
            dialogue.currentSentence = redEnding;
        }
        if (goal1.selectedSentence.questParameter == "orange")
        {
            dialogue.currentSentence = orangeEnding;
        }
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
        int result = greenQuestions - redQuestions; //Green is in favour and red is not
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
        dialogue.currentSentence.nextSentence = kingEnding;
        //complete king
    }

    void NeutralEnding()
    {
        dialogue.currentSentence.nextSentence = neutralEnding;
        AlchemistDialogue = Alchemist.GetComponent<DialogueManager>();
        AlchemistDialogue.currentSentence = AlchemistStartSentence;
    }

    void AgainstEnding()
    {
        dialogue.currentSentence.nextSentence = againstKingSentence;
        TheodoreDialogue = Theodore.GetComponent<DialogueManager>();
        TheodoreDialogue.currentSentence = TheodoreStartSentence;
    }

    void EnableAgainstFromNeutral()
    {
        if (withPotion)
        {
            TheodoreDialogue = Theodore.GetComponent<DialogueManager>();
            TheodoreDialogue.currentSentence = TheodoreStartSentence;
        }
        else
        {
            //complete quest;
            Debug.Log("Complete neutral");
        }
    }

    void AlchemistCompleted()
    {
        if (AlchemistGoal.selectedSentence.questParameter == "with potion")
        {
            dialogue.currentSentence = GauvainWithPotion;
            withPotion = true;
        }
        if (AlchemistGoal.selectedSentence.questParameter == "without potion")
        {
            dialogue.currentSentence = GauvainWithoutPotion;
            withPotion = false;
        }
    }

    void AgainstEndingCompleted()
    {
        //complete quest 
        Debug.Log("Complete against");
    }
}
