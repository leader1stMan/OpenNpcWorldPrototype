using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
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
    private SentenceGoal executedNobleGoal;
    private SentenceGoal riotWithNobleGoal;
    private SentenceGoal getRewardNoble;

    //List of sentences that changes the path of the dialogue
    public List<Sentence> againstNoble;
    public List<Sentence> withNoble;
    public List<Sentence> endTheodoreSequence;
    public List<Sentence> executedNobleFinal;
    public List<Sentence> endNobleSequence;
    public List<Sentence> endQuestWithNoble;

    //Start sentences of each section
    public Sentence goAwaySentence;
    public Sentence againstNobleSentence;
    public Sentence withNobleSentance;
    public Sentence theodoreStart;
    public Sentence executedNoble;
    public Sentence returnToNoble;

    public TextAsset speachAtCenter;
    public TextAsset executeNoble;
    public TextAsset nobleSpeach;

    private DialogueManager dialogue;
    public string path = null;

    public NPC Theodore;
    public GameObject Noble;

    public Transform CenterofTown;
    public Transform nobleHouse;
    public Transform runAwayArea;

    private GameObject target;

    public GameObject rioteer;
    public GameObject guard;

    public bool siegeNobel = false;

    public int numberOfGuardsDead = 0;
    public int numberOfRioteersDead = 0;

    public QuestState state;
    public enum QuestState { NotStarted, WithGaunavin, AttackNoble, ResumeNoblePower, AttackRiot, GuardBossFight, ExecuteNoble, NobleExecuted, ReturnToNoble, Complete };

    void Awake()
    {
        dialogue = GetComponent<DialogueManager>(); //DialogueManager executes the dialogue system

        stopNoblePower = new SentenceGoal(againstNoble); //stopNoblePower is the goal, againstNoble is the sentances
        stopNoblePower.AddHandler(GotoTheodore);
        
        resumeNobelPower = new SentenceGoal(withNoble);
        resumeNobelPower.AddHandler(GotoNoble);

        startRiotGoal = new SentenceGoal(endTheodoreSequence);
        startRiotGoal.AddHandler(StartRiotFunction);

        executedNobleGoal = new SentenceGoal(executedNobleFinal);
        executedNobleGoal.AddHandler(EndQuest);

        riotWithNobleGoal = new SentenceGoal(endNobleSequence);
        riotWithNobleGoal.AddHandler(StartRiotWithNoble);

        getRewardNoble = new SentenceGoal(endQuestWithNoble);
        getRewardNoble.AddHandler(getRewardFromNoble);
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponentInChildren<AnimationController>();

        Noble.GetComponent<NobleQuestScript>().rioteerLeader = gameObject;
    }

    bool calledWithNoble = false;

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

        if (state == QuestState.ResumeNoblePower && GetComponent<CharacterStats>().isDead && calledWithNoble == false)
        {
            calledWithNoble = true;
            Noble.GetComponent<DialogueManager>().currentSentence = withNobleSentance;
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
        state = QuestState.WithGaunavin;
        GetComponent<CharacterStats>().isInvincible = true;
        Theodore.GetComponent<CharacterStats>().isInvincible = true;
        Noble.GetComponent<CharacterStats>().isInvincible = true;

        dialogue.currentSentence = againstNobleSentence; 
        Theodore.GetComponent<DialogueManager>().currentSentence = theodoreStart;
    }

    void GotoNoble()
    {
        state = QuestState.ResumeNoblePower;
        dialogue.currentSentence = goAwaySentence;

        GetComponent<NPC>().enabled = false;
        //CombatBase combatBaseScript = GetComponent<CombatBase>().EnableCombat();
        //combatBaseScript.Tags.Add("Player");
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

    void StartRiotWithNoble()
    {
        StartCoroutine(ForStartRiotWithNoble());
    }

    IEnumerator ForStartRiotWithNoble()
    {
        yield return new WaitForSeconds(2f);

        StartCoroutine(Noble.GetComponent<NPC>().Conversation(null, AssetDatabase.GetAssetPath(nobleSpeach), this));
        SpawnSoldiers();
    }

    IEnumerator ReachedCenter()
    {
        SpawnSoldiers();
        yield return new WaitUntil(() => GetComponent<NavMeshAgent>().remainingDistance == 0);
        target = null;
        StartCoroutine(GetComponent<NPC>().Conversation(null, AssetDatabase.GetAssetPath(speachAtCenter), this));
    }

    public void EndConversation()
    {
        switch (state)
        {
            case QuestState.WithGaunavin:
                state = QuestState.AttackNoble;
                //CombatBase combatScript = GetComponent<CombatBase>().EnableCombat();
                //combatScript.attackPoint = nobleHouse;
                break;

            case QuestState.GuardBossFight:
                state = QuestState.ExecuteNoble;
                Noble.GetComponent<CharacterStats>().isInvincible = false;
                break;

            case QuestState.ResumeNoblePower:
                state = QuestState.AttackRiot;
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
        if (numberOfGuardsDead > 0)
        {
            state = QuestState.GuardBossFight;
            GetComponent<ShieldMeleeAI>().enabled = false;

            StartCoroutine(StartNobleExecution());
        }
    }

    public void NumberofRioteersDead()
    {
        numberOfRioteersDead++;
        if (numberOfRioteersDead > 0)
        {
            state = QuestState.ReturnToNoble;
            Noble.GetComponent<DialogueManager>().currentSentence = returnToNoble;
        }
    }

    IEnumerator StartNobleExecution()
    {
        GetComponent<NavMeshObstacle>().enabled = false;
        agent.enabled = true;
        agent.SetDestination(Noble.transform.position);
        yield return new WaitUntil(() => (Noble.transform.position - transform.position).magnitude <= 2);
        
        GetComponent<NPC>().isFirst = true;
        StartCoroutine(GetComponent<NPC>().Conversation(Noble, AssetDatabase.GetAssetPath(executeNoble), this));
        StartCoroutine(Noble.GetComponent<NPC>().Conversation(this.gameObject, null, null));
    }

    public IEnumerator NobleIsExecuted()
    {
        state = QuestState.NobleExecuted;

        target = FindObjectOfType<FirstPersonAIO>().gameObject;
        agent.SetDestination(target.transform.position);
        yield return new WaitUntil(() => GetComponent<NavMeshAgent>().remainingDistance <= 2);

        agent.SetDestination(transform.position);

        GetComponent<DialogueManager>().currentSentence = executedNoble;
        target.GetComponent<PlayerActions>().StartConversation(gameObject);

        enabled = false;
    }

    void getRewardFromNoble()
    {
        Debug.Log("quest finished!");
    }

    void EndQuest()
    {
        state = QuestState.Complete;
    }
}
