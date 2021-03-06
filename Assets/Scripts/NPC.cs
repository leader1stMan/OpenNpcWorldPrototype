using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NPC : NpcData
{
    public bool ShowDebugMessages;

    public NavMeshAgent agent { get; private set; }
    private Animator anim;
    private DialogueManager dialogue;
    [SerializeField] private float speedAnimDevider = 1;
    [SerializeField] private float stopDistance;
    [SerializeField] private float stopDistanceRandomAdjustment;
   
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        dialogue = GetComponent<DialogueManager>();  

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
    }

    private void SetMoveTarget(Transform target)
    {
        agent.ResetPath();
        agent.SetDestination(target.position);
    }

    private void GoToWork()
    {
        if(dialogue._isdialogue == false)
        {
            if (currentState == NpcStates.GoingToWork)
                return;

            currentState = NpcStates.GoingToWork;
            SetMoveTarget(work);
            if(ShowDebugMessages)
            Debug.Log(name + " is going to work");
        }
    }

    private void GoHome()
    {
        if(dialogue._isdialogue == false)
        {
            if (currentState == NpcStates.GoingHome)
                return;

            currentState = NpcStates.GoingHome;
            anim.SetBool("Working", false);

            SetMoveTarget(home);
            if(ShowDebugMessages)
            Debug.Log(name + " is going home");
        }
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
