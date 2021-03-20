using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NPC : NpcData
{
    public bool ShowDebugMessages;

    public NavMeshAgent agent { get; private set; }
    private Animator anim;

    [SerializeField] private float speedAnimDevider = 1;
    [SerializeField] private float stopDistance;
    [SerializeField] private float stopDistanceRandomAdjustment;
   
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
    }

    void Update()
    {
        anim.SetFloat("InputMagnitude", agent.velocity.magnitude / speedAnimDevider);

        if((currentState == NpcStates.GoingToWork && Vector3.Distance(transform.position, work.position) < stopDistance )|| (agent.remainingDistance == 0 && !agent.pathPending))
        {
            if(ShowDebugMessages)
            Debug.Log("StartingToWork");
            currentState = NpcStates.Working;
            anim.SetBool("Working", true);
        }

        Debug.Log(gameObject.name + " is " + currentState);
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
                NpcStates state = col.gameObject.GetComponent<NPC>().currentState;
                if (state == NpcStates.Attacking || state == NpcStates.Defending)
                {
                    ChangeState(NpcStates.Scared);
                }
            }
            // If the NPC is looking at an enemy Attacking or defending, run
            else if (col.gameObject.GetComponent<EnemyBase>())
            {
                EnemyState state = col.gameObject.GetComponent<EnemyBase>().CurrentState;
                Debug.Log(col.gameObject.name + " is " + state);
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
        switch (NewState)
        {
            case NpcStates.Scared:
                agent.speed *= 4;
                SetMoveTarget(home);
                break;
            case NpcStates.GoingHome:
                if(PrevState == NpcStates.Scared) agent.speed *= .25f;
                SetMoveTarget(home);
                break;
            case NpcStates.GoingToWork:
                if (PrevState == NpcStates.Scared) agent.speed *= .25f;
                SetMoveTarget(work);
                break;
            case NpcStates.Idle:
                if (PrevState == NpcStates.Scared) agent.speed *= .25f;
                float time = FindObjectOfType<DayAndNightControl>().currentTime;
                if(time > .3f && time < .8f)
                {
                    GoToWork();
                }
                else
                {
                    GoHome();
                }
                break;
            case NpcStates.InteractingWithPlayer:
                if (PrevState == NpcStates.Scared) agent.speed *= .25f;
                break;
            case NpcStates.Working:
                if (PrevState == NpcStates.Scared) agent.speed *= .25f;
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

    private void GoToWork()
    {
        if (currentState == NpcStates.GoingToWork || currentState == NpcStates.Scared)
            return;

        currentState = NpcStates.GoingToWork;
        SetMoveTarget(work);
        if(ShowDebugMessages)
        Debug.Log(name + " is going to work");
    }

    private void GoHome()
    {
        if (currentState == NpcStates.GoingHome || currentState == NpcStates.Scared)
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
}
