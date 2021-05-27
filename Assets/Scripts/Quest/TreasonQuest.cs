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
    public string path = null;
    private TMP_Text text; 
    public bool isFirst;

    public NPC Theodore;
    public GameObject Noble;

    public Transform CenterofTown;
    public Transform nobleHouse;

    private GameObject target;

    public GameObject rioteer;
    public GameObject guard;

    public bool siegeNobel = false;

    public int numberOfGuardsDead = 0;

    public QuestState state;
    public enum QuestState { NotStarted, TalkingWithGaunavin, AttackingNobleHouse, GuardBossFight, Complete };

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

        Noble.GetComponent<NobleQuestScript>().rioteerLeader = gameObject;
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
        SpawnSoldiers();
        yield return new WaitUntil(() => GetComponent<NavMeshAgent>().remainingDistance == 0);
        target = null;
        StartCoroutine(Conversation(0));
    }

    IEnumerator Conversation(int n, bool talkToNoble = false)
    {
        if (!talkToNoble)
        {
            StreamReader reader;
            string line;
            List<string> lines = new List<string>();

            path = DialoguePaths[n];

            reader = new StreamReader(path);
            //Converstion is sotred in .txt file. "{}" separates first and second NPC's part 
            while ((line = reader.ReadLine()) != "{}")
            {
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
        }
        else
        {
            StreamReader reader;
            NobleQuestScript npc = Noble.GetComponent<NobleQuestScript>();
            string line;
            List<string> lines = new List<string>();

            // if NPC is first, it randomly chooses conversation and assigns it to the second NPC
            // if NPC is second, it waits till conversation is assigned to him by the first NPC
            if (isFirst)
            {
                //Assigning the conversation to npc1, npc2
                path = DialoguePaths[n];
                npc.path = path;

                reader = new StreamReader(path);
                //Converstion is sotred in .txt file. "{}" separates first and second NPC's part 
                while ((line = reader.ReadLine()) != "{}")
                {
                    lines.Add(line); //Storing the conversation by each line
                }
            }
            else
            {
                yield return new WaitUntil(() => path != null); //Wait till first NPC sends the conversation he chose
                reader = new StreamReader(path);
                while (reader.ReadLine() != "{}") ;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            text.text = null; //Ui for showing text
            yield return new WaitUntil(() => isFirst); //We now don't need 'isFirst' to tell who started the conversation
            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].StartsWith(" ")) //Displays sentece for 4 secs
                {
                    text.text = lines[i];
                    yield return new WaitForSeconds(4);
                    text.text = null;
                }
                isFirst = false; //Turns 'isFirst' to false while pturning on it for the talker
                npc.isFirst = true; //Indicating that it's talker's time to speak
                yield return new WaitUntil(() => isFirst);
            }
            npc.isFirst = true;
        }
    
        EndConversation(n);
    }

    void EndConversation(int n)
    {
        agent.isStopped = false;
        StopCoroutine("Conversation");
        StopCoroutine("RotateTo");
        path = null;
        isFirst = false;
        text.text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();

        switch(n)
        {
            case 0:
                state = QuestState.AttackingNobleHouse;
                CombatBase combatScript = GetComponent<CombatBase>().EnableCombat();
                combatScript.attackPoint = nobleHouse;
                break;
        }
    }

    void SpawnSoldiers()
    {
        for(int i = 0; i < 5; i++)
        {
            Vector3 position = new Vector3(CenterofTown.position.x + Random.Range(-10.0F, 10.0F), CenterofTown.position.y, CenterofTown.position.z + Random.Range(-10.0F, 10.0F));
            Instantiate(rioteer, position, Quaternion.identity);
            Vector3 position1 = new Vector3(nobleHouse.position.x + Random.Range(-10.0F, 10.0F), nobleHouse.position.y, nobleHouse.position.z + Random.Range(-10.0F, 10.0F));
            Instantiate(guard, position1, Quaternion.identity);
        }
    }

    public void SpawnRioteerAgain()
    {
        Vector3 position = new Vector3(CenterofTown.position.x + Random.Range(-10.0F, 10.0F), CenterofTown.position.y, CenterofTown.position.z + Random.Range(-10.0F, 10.0F));
        Instantiate(rioteer, position, Quaternion.identity);
    }

    public void SpawnGuardAgain()
    {
        Vector3 position1 = new Vector3(nobleHouse.position.x + Random.Range(-10.0F, 10.0F), nobleHouse.position.y, nobleHouse.position.z + Random.Range(-10.0F, 10.0F));
        Instantiate(guard, position1, Quaternion.identity);
    }

    public void NumberofGuardsDead()
    {
        numberOfGuardsDead++;
        if (numberOfGuardsDead > 1)
        {
            state = QuestState.GuardBossFight;
            GetComponent<ShieldMeleeAI>().enabled = false;

            isFirst = true;
            StartCoroutine(StartNobleExecution());
        }
    }

    IEnumerator StartNobleExecution()
    {
        agent.SetDestination(Noble.transform.position);
        yield return new WaitUntil(() => (Noble.transform.position - transform.position).magnitude <= 1);

        StartCoroutine(Conversation(1, true));
        Noble.GetComponent<NobleQuestScript>().StartConversation(1, true);
    }
}
