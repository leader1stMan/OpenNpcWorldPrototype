using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NPC : NpcData, IAttackable
{
    public bool ShowDebugMessages;
    
    public NavMeshAgent agent { get; private set; }
    private Animator anim;

    public GameObject Attacker;
    public bool isAttacked;
    public float movementSpeed;
    public float scaredRunningSpeed;
    public float scaredAcceleration;
    public float runningDistance;
    public float runningTime;
    private float timeToRun;
    public float NavMeshAccurancy;
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

        stopDistance = UnityEngine.Random.Range(stopDistanceRandomAdjustment + stopDistance, stopDistance);
        agent.stoppingDistance = stopDistance;

        GoHome();
    }
    void Update()
    {
        anim.SetFloat("InputMagnitude", agent.velocity.magnitude / speedAnimDevider);

        if (timeToRun > 0)
        {
            timeToRun -= Time.deltaTime;
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
                if (npc.isAttacked)
                {
                    Attacker = npc.Attacker;
                    ChangeState(NpcStates.Scared);
                }
            }
            // If the NPC is looking at an enemy Attacking or defending, run
            else if (col.gameObject.GetComponent<EnemyBase>())
            {
                EnemyBase enemy= col.gameObject.GetComponent<EnemyBase>();
                EnemyState state = enemy.CurrentState;
                if (state == EnemyState.Attacking)
                {
                    Attacker = col.gameObject;
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
        {
            anim.SetBool("Working", false);
        }
        switch (NewState)
        {
            case NpcStates.Scared:
                StartCoroutine("Run", Attacker);
                break;
            case NpcStates.GoingHome:
                GoHome();
                break;
            case NpcStates.GoingToWork:
                GoToWork();
                break;
            case NpcStates.Idle:
                float time = FindObjectOfType<DayAndNightControl>().currentTime;
                if (time > .2f && time < .8f)
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
        Vector2 vector = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 randomDirection = new Vector3(vector.x, transform.position.y, vector.y) * runningDistance;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, runningDistance, 1);
        agent.SetDestination(hit.position);
    }

    void GoToWork()
    {
        if (currentState == NpcStates.GoingToWork || currentState == NpcStates.Scared)
            return;
        StartCoroutine("GoToWorkCoroutine");
    }
    IEnumerator GoToWorkCoroutine()
    {
        agent.speed = movementSpeed;
        currentState = NpcStates.GoingToWork;
        SetMoveTarget(work);

        yield return new WaitUntil(() => Vector3.Distance(agent.destination, transform.position) <= 0.05f);

        if (currentState == NpcStates.Working)
        ChangeState(NpcStates.Working);
    }

    void GoHome()
    {
        if (currentState == NpcStates.GoingHome || currentState == NpcStates.Scared)
            return;
        StartCoroutine("GoHomeCoroutine");
    }

    IEnumerator GoHomeCoroutine()
    {
        agent.speed = movementSpeed;
        currentState = NpcStates.GoingHome;

        SetMoveTarget(home);

        yield return new WaitUntil(() => Vector3.Distance(agent.destination, transform.position) <= 0.05f);
        if (currentState == NpcStates.GoingHome)
            ChangeState(NpcStates.Idle);
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

    public void OnAttack(GameObject attacker, Attack attack)
    {
        Attacker = attacker;
        ChangeState(NpcStates.Scared);
        StartCoroutine("Attacked");
    }

    IEnumerator Attacked()
    {
        isAttacked = true;
        yield return new WaitForSeconds(1f);
        isAttacked = false;
    }
    IEnumerator Run(GameObject attacker)
    {
        float currentAcceleration = agent.acceleration;
        agent.speed = scaredRunningSpeed;
        timeToRun = runningTime;
        int interaction = 0;
        while (timeToRun > 0)
        {
            interaction++;
            Vector3 distanceIn3D = attacker.transform.position - transform.position;
            float magnitude = new Vector2(distanceIn3D.x, distanceIn3D.z).magnitude;
            Vector2 distance = new Vector2(distanceIn3D.x / magnitude, distanceIn3D.z / magnitude);
            Debug.DrawLine(transform.position, new Vector3(transform.position.x + distance.x * runningDistance, transform.position.y, transform.position.z + distance.y * runningDistance), Color.blue, 20f);
            Vector3 goal;
            NavMeshHit hit, temp;
            int index = 0;
            double angleX = Math.Acos(distance.x);
            double angleY = Math.Asin(distance.y);
            bool isPathValid;
            do
            {
                angleX += index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                angleY -= index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                distance = new Vector2((float)Math.Cos(angleX), (float)Math.Sin(angleY));
                index++;
                goal = new Vector3(transform.position.x - distance.x * runningDistance, transform.position.y, transform.position.z - distance.y * runningDistance);
             
                Debug.DrawLine(transform.position, goal, Color.red, 20f);
                Debug.DrawLine(goal, goal + new Vector3(0.05f, 0, 0.05f), Color.blue , 20f);
                NavMesh.SamplePosition(goal, out hit, runningDistance/5, agent.areaMask);
                yield return new WaitUntil(() => !agent.pathPending);
                agent.SetDestination(hit.position);
                float length = GetPathLength(agent.path);
                isPathValid = (length >= runningDistance) && index <= 13;

                Debug.Log(length + " " + runningDistance);
            } while (isPathValid);
            if (timeToRun < 2f)
                agent.acceleration = currentAcceleration;
            yield return new WaitUntil(() => Vector3.Distance(agent.destination, transform.position) <= runningDistance / 1.2);
            if (interaction == 1)
                agent.acceleration = scaredAcceleration;
        }
        ChangeState(NpcStates.Idle);
    }
    public float GetPathLength(NavMeshPath path)
    {
        float lng = 0.0f;

        if ((path.status != NavMeshPathStatus.PathInvalid) && (path.corners.Length > 1))
        {
            for (int i = 1; i < path.corners.Length; ++i)
            {
                lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
        }
        return lng;
    }
}