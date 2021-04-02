using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class NPC : NpcData, IAttackable
{
    public bool ShowDebugMessages;

    public NavMeshAgent agent { get; private set; }
    private Animator anim;

    public bool isAttacked;
    public float movementSpeed = 2;
    public float scaredRunningSpeed = 4;
    public float runningDistance = 40;
    [SerializeField] private float speedAnimDevider = 1;
    [SerializeField] private float stopDistance;
    [SerializeField] private float stopDistanceRandomAdjustment;

    public int priority;
    private bool isFirst;
    private int Dialogue;
    private bool isStarted;
    private int called;
    protected GameObject conversatingWith;
    protected NPC NPCscript;

    private TMP_Text text;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        /*____________________Might need to change this as this is called every frame, interfering with the code______________________*/
        FindObjectOfType<DayAndNightControl>().OnMorningHandler += GoToWork;
        FindObjectOfType<DayAndNightControl>().OnEveningHandler += GoHome;

        stopDistance = Random.Range(stopDistanceRandomAdjustment + stopDistance, stopDistance);
        agent.stoppingDistance = stopDistance;

        GoHome();

        text = GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        anim.SetFloat("InputMagnitude", agent.velocity.magnitude / speedAnimDevider);

        if (currentState == NpcStates.IsTalking)
        {
            if (!isStarted)
            {
                isStarted = true;
                StartCoroutine("Conversation");
            }
        }
        else
        {
            isStarted = false;
            StopCoroutine("Conversation");

            if (currentState == NpcStates.GoingToWork && Vector3.Distance(transform.position, work.position) <= stopDistance)
            {
                if (ShowDebugMessages)
                    Debug.Log("StartingToWork");
                ChangeState(NpcStates.Working);
            }
        }

        WatchEnvironment();
    }

    private void WatchEnvironment()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, VisionLayers);

        foreach (Collider col in cols)
        {
            // If the NPC is looking at another NPC attacking or defending, run
            if (col.gameObject.GetComponent<NPC>())
            {
                NPC npc = col.gameObject.GetComponent<NPC>();
                NpcStates state = npc.currentState;
                if (state == NpcStates.Attacking || state == NpcStates.Defending || npc.isAttacked)
                {
                    ChangeState(NpcStates.Scared);
                }
            }
            // If the NPC is looking at an enemy Attacking or defending, run
            else if (col.gameObject.GetComponent<EnemyBase>())
            {
                EnemyState state = col.gameObject.GetComponent<EnemyBase>().CurrentState;
                if (state == EnemyState.Attacking)
                {
                    ChangeState(NpcStates.Scared);
                }
            }
        }
    }

    private void ChangeState(NpcStates NewState)
    {
        if (currentState == NewState)
            return;

        NpcStates PrevState = currentState;

        currentState = NewState;
        OnStateChanged(PrevState, NewState);
    }

    private void OnStateChanged(NpcStates PrevState, NpcStates NewState)
    {
        if (PrevState == NpcStates.Working)
            anim.SetBool("Working", false);

        switch (NewState)
        {
            case NpcStates.Scared:
                StartCoroutine("Run");
                break;
            case NpcStates.GoingHome:
                agent.speed = movementSpeed;
                SetMoveTarget(home);
                break;
            case NpcStates.GoingToWork:
                agent.speed = movementSpeed;
                SetMoveTarget(work);
                break;
            case NpcStates.Idle:
                agent.speed = movementSpeed;
                float time = FindObjectOfType<DayAndNightControl>().currentTime;
                if (time > .3f && time < .8f)
                {
                    GoToWork();
                }
                else
                {
                    GoHome();
                }
                break;
            case NpcStates.InteractingWithPlayer:
                agent.speed = movementSpeed;
                break;
            case NpcStates.IsTalking:
                agent.speed = movementSpeed;
                SetMoveTarget(conversatingWith.transform);
                break;
            case NpcStates.Working:
                agent.speed = movementSpeed;
                anim.SetBool("Working", true);
                SetMoveTarget(work);
                break;
            default: break;
        }
    }

    private void SetMoveTarget(Transform target)
    {
        agent.ResetPath();
        agent.SetDestination(target.position);
    }

    private void SetRandomMoveTarget()
    {
        agent.ResetPath();
        Vector2 vector = Random.insideUnitCircle.normalized;
        Vector3 randomDirection = new Vector3(vector.x, transform.position.y, vector.y) * runningDistance;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, runningDistance, 1);
        agent.SetDestination(hit.position);
    }

    private void GoToWork()
    {
        if (currentState == NpcStates.GoingToWork || currentState == NpcStates.Working || currentState == NpcStates.IsTalking || currentState == NpcStates.Scared)
            return;

        currentState = NpcStates.GoingToWork;
        SetMoveTarget(work);
        if(ShowDebugMessages)
        Debug.Log(name + " is going to work");
    }

    private void GoHome()
    {
        if (currentState == NpcStates.GoingHome || currentState == NpcStates.IsTalking || currentState == NpcStates.Scared)
            return;

        currentState = NpcStates.GoingHome;
        anim.SetBool("Working", false);

        SetMoveTarget(home);
        if(ShowDebugMessages)
        Debug.Log(name + " is going home");
    }

    private void OnDestroy()
    {
        try
        {
            FindObjectOfType<DayAndNightControl>().OnMorningHandler -= GoToWork;
            FindObjectOfType<DayAndNightControl>().OnEveningHandler -= GoHome;
        }
        catch
        {
            if (ShowDebugMessages)
                Debug.LogWarning("DayAndNightControl object is not found. This is ok if the scene is unloaded.");
        }
    }

    IEnumerator Conversation()
    {
        string path;

        switch (Random.Range(1, 2))
        {
            case 1:
                if (isFirst)
                {
                    path = "Assets/NPC dialogues/Dialogue 1a.txt";
                }
                else
                {
                    path = "Assets/NPC dialogues/Dialogue 1b.txt";
                }
                break;
            case 2:
                if (isFirst)
                {
                    path = "Assets/NPC dialogues/Dialogue 2a.txt";
                }
                else
                {
                    path = "Assets/NPC dialogues/Dialogue 2b.txt";
                }
                break;
            default:
                path = null;
                break;
        }

        if (!isFirst)
        {
            text.text = null;
            yield return new WaitForSeconds(4);
        }

        string line;
        StreamReader reader = new StreamReader(path);
        while ((line = reader.ReadLine()) != null)
        {
            text.text = line;
            yield return new WaitForSeconds(4);
            text.text = null;
            yield return new WaitForSeconds(4);
        }

        if (isFirst)
        {
            yield return new WaitForSeconds(4);
        }

        //Debug.Log("Conversation ended by" + gameObject.name);
        isFirst = false;
        ChangeState(NpcStates.Idle);
        conversatingWith = null;

        text.text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
        yield return null;
    }

    void OnTriggerStay(Collider other)
    {
        if (currentState != NpcStates.Scared || currentState != NpcStates.IsTalking)
        {
            if (other.CompareTag("Npc"))
            {
                conversatingWith = other.gameObject;
                NPCscript = conversatingWith.GetComponentInParent<NPC>();
                if (NPCscript.currentState != NpcStates.Scared || NPCscript.currentState != NpcStates.IsTalking)
                {
                    if (priority > NPCscript.priority)
                    {
                        if (Random.Range(0, 1000) == 1)
                        {
                            ChangeState(NpcStates.IsTalking);
                            isFirst = true;
                            NPCscript.ChangeState(NpcStates.IsTalking);
                            NPCscript.isFirst = false;
                            NPCscript.conversatingWith = this.gameObject;
                            //Debug.Log("Conversation started by " + gameObject.name);
                        }
                    }
                }
            }
        }
    }

    public void OnAttack(GameObject attacker, Attack attack)
    {
        ChangeState(NpcStates.Scared);
        StartCoroutine("Attacked");
    }

    IEnumerator Attacked()
    {
        isAttacked = true;
        yield return new WaitForSeconds(1f);
        isAttacked = false;
    }

    IEnumerator Run()
    {
        agent.speed = scaredRunningSpeed;
        SetRandomMoveTarget();
        yield return new WaitUntil(() => Vector3.Distance(agent.destination, transform.position) <= 0.5f);
        ChangeState(NpcStates.Idle);
    }
}
