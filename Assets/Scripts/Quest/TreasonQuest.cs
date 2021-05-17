using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using TMPro;

public class TreasonQuest : Quest
{
    private AnimationController controller;
    public NavMeshAgent agent { get; private set; }

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
    public List<string> DialoguePaths;
    string path = null;
    private TMP_Text text;

    public NPC Theodore;
    public Transform CenterofTown;

    private GameObject target;

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

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponentInChildren<AnimationController>();
        text = GetComponentInChildren<TMP_Text>();
    }

    private void Update()
    {
        //Manage animations
        if (agent.velocity.magnitude == 0)
        {
            //Idle animation if npc isn't moving
            controller.ChangeAnimation(AnimationController.IDLE, AnimatorLayers.ALL);
        }
        else
        {
            if (agent.velocity.magnitude < 2.5f)
            {
                //Walk animation if npc is moving slow
                controller.ChangeAnimation(AnimationController.WALK, AnimatorLayers.ALL);
            }
            else
            {
                //Walk animation if npc is moving fast
                controller.ChangeAnimation(AnimationController.RUN, AnimatorLayers.ALL);
            }
        }

        if (target != null)
        {
            RotateTo(target);
        }
    }

    void RotateTo(GameObject target)
    {
        Quaternion lookRotation;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / (Quaternion.Angle(transform.rotation, lookRotation) / agent.angularSpeed));
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

        GetComponent<NavMeshAgent>().isStopped = false;
        GetComponent<NavMeshAgent>().SetDestination(CenterofTown.position);
        target = CenterofTown.gameObject;
        StartCoroutine(ReachedCenter());
    }

    IEnumerator ReachedCenter()
    {
        yield return new WaitUntil(() => GetComponent<NavMeshAgent>().remainingDistance == 0);
        target = null;
        StartCoroutine(Conversation(0));
    }

    IEnumerator Conversation(int n)
    {
        StreamReader reader;
        string line;
        List<string> lines = new List<string>();

        path = DialoguePaths[0];

        reader = new StreamReader(path);
        //Converstion is sotred in .txt file. "{}" separates first and second NPC's part 
        while ((line = reader.ReadLine()) != "{}")
        {
            Debug.Log(true);
            lines.Add(line); //Storing the conversation by each line
        }

        text.text = null; //Ui for showing text
        for (int i = 0; i < lines.Count; i++)
        {
            if (!lines[i].StartsWith(" ")) //Displays sentece for 4 secs
            {
                text.text = lines[i];
                yield return new WaitForSeconds(4);
                text.text = null;
            }
        }
        EndConversation();
    }

    void EndConversation()
    {
        agent.isStopped = false;
        StopCoroutine("Conversation");
        StopCoroutine("RotateTo");
        path = null;
        text.text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
    }
}
