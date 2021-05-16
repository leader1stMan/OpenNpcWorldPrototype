using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TreasonQuest : Quest
{
    //Handlers are added to goals and are executed when a goal is complete
    private SentenceGoal stopNoblePower;
    private SentenceGoal resumeNobelPower;
    private SentenceGoal startRiotGoal;

    //List of sentences that changes the path of the dialogue
    public List<Sentence> againstNoble;
    public List<Sentence> withNoble;
    public List<Sentence> endTheodoreSequence;

    //Start sentences of each section
    public Sentence goAwaySentence;
    public Sentence againstNobleSentence;
    public Sentence theodoreStart;

    private DialogueManager dialogue;
    
    public NPC Theodore;
    public Transform CenterofTown;

    void Awake()
    {
        dialogue = GetComponent<DialogueManager>(); //DialogueManager executes the dialogue system

        stopNoblePower = new SentenceGoal(againstNoble);
        stopNoblePower.AddHandler(GotoTheodore);
        
        resumeNobelPower = new SentenceGoal(withNoble);
        resumeNobelPower.AddHandler(GotoNoble);

        startRiotGoal = new SentenceGoal(endTheodoreSequence);
        startRiotGoal.AddHandler(StartRiotFunction);
    }

    void GotoTheodore()
    {
        dialogue.currentSentence = againstNobleSentence; 
        Theodore.GetComponent<DialogueManager>().currentSentence = theodoreStart;
    }

    void GotoNoble()
    {
        dialogue.currentSentence = goAwaySentence;
    }

    void StartRiotFunction()
    {
        GetComponent<NPC>().enabled = false;
        CombatBase[] combatBases = GetComponents<CombatBase>();
        foreach (CombatBase combatBase in combatBases)
        {
            combatBase.enabled = false;
        }

        GetComponent<NavMeshAgent>().SetDestination(CenterofTown.position);
    }
}
