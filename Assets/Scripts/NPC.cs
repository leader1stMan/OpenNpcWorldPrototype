using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using TMPro;

public class NPC : NpcData, IAttackable
{
    public bool ShowDebugMessages;
    
    public NavMeshAgent agent { get; private set; }
    private Animator animator;
    private string currentAnimation;

    public GameObject Attacker;
    public bool isAttacked;
    public float movementSpeed;
    public float scaredRunningSpeed;
    public float scaredAcceleration;
    public float runningDistance;
    public float runningTime;
    private float timeToRun;
    [SerializeField] private float speedAnimDevider = 1;

    private bool isFirst;
    string path = null;

    private TMP_Text text;

    private AnimationController controller;

    public List<string> DialoguePaths;

    Rigidbody[] rig;
    SkinnedMeshRenderer[] skin;

    public bool combatState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        controller = GetComponentInChildren<AnimationController>();

        /*____________________Might need to change this as this is called every frame, interfering with the code______________________*/
        FindObjectOfType<DayAndNightControl>().OnMorningHandler += GoToWork;
        FindObjectOfType<DayAndNightControl>().OnEveningHandler += GoHome;

        text = GetComponentInChildren<TMP_Text>();

        skin = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinned in skin)
        {
            skinned.updateWhenOffscreen = false;
        }

        rig = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rig)
        {
            if (rigidbody != this.GetComponent<Rigidbody>())
            {
                rigidbody.GetComponent<Collider>().enabled = false;
                rigidbody.isKinematic = true;
            }
        }

        GetComponent<CapsuleCollider>().enabled = true;

        combatState = false;
        GetComponent<EnemyBase>().enabled = false;
    }
    void Update()
    {
        if (timeToRun > 0)
        {
            timeToRun -= Time.deltaTime;
        }

        if (combatState)
            ChangeState(NpcStates.Combat);
        WatchEnvironment();

        if (GetComponent<CharacterStats>().isDead)
        {
            ChangeState(NpcStates.Dead);
        }
    }

    void FixedUpdate()
    {
        if (currentState != NpcStates.Combat)
        {
            if (agent.velocity.magnitude == 0)
            {
                controller.ChangeAnimation(AnimationController.IDLE, AnimatorLayers.ALL);
            }
            else
            {
                if (agent.velocity.magnitude < 2.5f)
                {
                    controller.ChangeAnimation(AnimationController.WALK, AnimatorLayers.ALL);
                }
                else
                {
                    controller.ChangeAnimation(AnimationController.RUN, AnimatorLayers.ALL);
                }
            }
        }
    }

    private void WatchEnvironment()
    {
        if (currentState != NpcStates.Combat)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, VisionLayers);

            foreach (Collider col in cols)
            {
                // If the NPC is looking at another NPC attacking or defending, run
                if (col.gameObject.GetComponent<NPC>())
                {
                    NPC npc = col.gameObject.GetComponent<NPC>();
                    if (npc.isAttacked)
                    {
                        Attacker = npc.Attacker;
                        ChangeState(NpcStates.Scared);
                    }
                }
                // If the NPC is looking at an enemy Attacking or defending, run
                else if (col.gameObject.GetComponent<EnemyBase>())
                {
                    EnemyBase enemy = col.gameObject.GetComponent<EnemyBase>();
                    EnemyState state = enemy.CurrentState;
                    if (state == EnemyState.Attacking)
                    {
                        Attacker = col.gameObject;
                        ChangeState(NpcStates.Scared);
                    }
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
       switch (PrevState)
        {
            case NpcStates.GoingHome:
                StopGoingHome();
                break;
            case NpcStates.GoingToWork:
                StopGoingToWork();
                break;
            case NpcStates.Working:
                break;
            case NpcStates.Talking:
                EndConversation();
                break;
            case NpcStates.Combat:
                GetComponent<EnemyBase>().enabled = false;
                break;
            default:
                break;
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
                if (time > .3f && time < .7f)
                {
                    GoToWork();
                }
                else
                {
                    GoHome();
                }
                break;
            case NpcStates.InteractingWithPlayer:
                break;
            case NpcStates.Talking:
                agent.isStopped = true;
                break;
            case NpcStates.Working:
                SetMoveTarget(work);
                break;
            case NpcStates.Combat:
                GetComponent<EnemyBase>().enabled = true;
                break;
            case NpcStates.Dead:
                foreach (SkinnedMeshRenderer skinned in skin)
                {
                    skinned.updateWhenOffscreen = true;
                }

                GetComponentInChildren<Animator>().enabled = false;
                agent.enabled = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = false;

                foreach (Rigidbody rigidbody in rig)
                {
                    if (rigidbody != this.GetComponent<Rigidbody>())
                    {
                        rigidbody.GetComponent<Collider>().enabled = true;
                        rigidbody.isKinematic = false;
                    }
                }
                break;
            default: break;
        }
    }

    private void SetMoveTarget(Transform target)
    {
        agent.ResetPath();
        agent.SetDestination(target.position);
    }

    void GoToWork()
    {
        if (currentState == NpcStates.GoingToWork || currentState == NpcStates.Working || currentState == NpcStates.Talking || currentState == NpcStates.Scared || currentState == NpcStates.Combat)
            return;
        StartCoroutine("GoToWorkCoroutine");
    }
    IEnumerator GoToWorkCoroutine()
    {
        agent.speed = movementSpeed;
        ChangeState(NpcStates.GoingToWork);
        SetMoveTarget(work);
        yield return new WaitUntil(() => agent.remainingDistance <= 0.1f && !agent.pathPending);

        if (currentState != NpcStates.Working)
        ChangeState(NpcStates.Working);
    }

    void StopGoingToWork()
    {
        agent.ResetPath();
        StopCoroutine("GoToWorkCoroutine");
    }

    void GoHome()
    {
        if (currentState == NpcStates.GoingHome || currentState == NpcStates.Talking || currentState == NpcStates.Scared || currentState == NpcStates.Combat)
            return;
        StartCoroutine("GoHomeCoroutine");
    }

    void StopGoingHome()
    {
        agent.ResetPath();
        StopCoroutine("GoHomeCoroutine");
    }

    IEnumerator GoHomeCoroutine()
    {
        agent.speed = movementSpeed;
        ChangeState(NpcStates.GoingHome);

        SetMoveTarget(home);

        yield return new WaitUntil(() => agent.remainingDistance <= 0.1f && !agent.pathPending);
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

    IEnumerator Conversation(GameObject talker)
    {
        ChangeState(NpcStates.Talking);
        StartCoroutine("RotateTo", talker);

        StreamReader reader;
        NPC npc = talker.GetComponent<NPC>();
        string line;
        List<string> lines = new List<string>();
        if (isFirst)
        {
            path = AssignPath();
            npc.path = path;
            reader = new StreamReader(path);
            while ((line = reader.ReadLine()) != "{}")
            {
                lines.Add(line);
            }
        }
        else
        {
            yield return new WaitUntil(() => path != null);
             reader = new StreamReader(path);
            while (reader.ReadLine() != "{}") ;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }

        text.text = null;
        yield return new WaitUntil(() => isFirst);
        for (int i = 0; i < lines.Count; i++)
        {
            if (!lines[i].StartsWith(" "))
            {
                text.text = lines[i];
                yield return new WaitForSeconds(4);
                text.text = null;
            }
            isFirst = false;
            npc.isFirst = true;
            yield return new WaitUntil(() => isFirst);
        }
        npc.isFirst = true;
        EndConversation();
    }

    string AssignPath()
    {
        if (DialoguePaths.Count > 0)
            return DialoguePaths[UnityEngine.Random.Range(0, DialoguePaths.Count - 1)];
        else
            return null;
    }
    public void StartConversation(GameObject talker)
    {
        StartCoroutine("Conversation", talker);
    }
    public void EndConversation()
    {
        agent.isStopped = false;
        StopCoroutine("Conversation");
        StopCoroutine("RotateTo");
        path = null;
        isFirst = false;
        text.text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
        if (currentState == NpcStates.Talking)
            ChangeState(NpcStates.Idle);
    }
    void OnTriggerStay(Collider other)
    {
        if (currentState == NpcStates.Scared || currentState == NpcStates.Talking || currentState == NpcStates.Combat)
            return;
        if (!other.CompareTag("Npc"))
            return;
        NPC NPCscript = other.GetComponentInParent<NPC>();
        if (NPCscript.currentState == NpcStates.Scared || NPCscript.currentState == NpcStates.Talking || NPCscript.currentState == NpcStates.Combat)
            return;
        if (UnityEngine.Random.Range(0, 1000) == 1)
        {
            if (GetInstanceID() > NPCscript.GetInstanceID())
            {
                isFirst = true;
                NPCscript.isFirst = false;
                StartConversation(other.gameObject);
                NPCscript.StartConversation(gameObject);
            }
        }
    }    

    public void OnAttack(GameObject attacker, Attack attack)
    {
        Attacker = attacker;
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
        agent.ResetPath();
        while (timeToRun > 0)
        {
            interaction++;
            Vector3 distanceIn3D = attacker.transform.position - transform.position;
            float magnitude = new Vector2(distanceIn3D.x, distanceIn3D.z).magnitude;
            Vector2 distance = new Vector2(distanceIn3D.x / magnitude, distanceIn3D.z / magnitude);
            //Debug.DrawLine(transform.position, new Vector3(transform.position.x + distance.x * runningDistance, transform.position.y, transform.position.z + distance.y * runningDistance), Color.blue, 20f);
            Vector3 goal;
            NavMeshHit hit;
            int index = 0;
            double angleX = Math.Acos(distance.x);
            double angleY = Math.Asin(distance.y);
            bool isPathValid;
            NavMeshPath path = new NavMeshPath();
            do
            {
                angleX += index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                angleY -= index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                distance = new Vector2((float)Math.Cos(angleX), (float)Math.Sin(angleY));
                index++;
                goal = new Vector3(transform.position.x - distance.x * runningDistance, transform.position.y, transform.position.z - distance.y * runningDistance);
                bool samplePosition;
                samplePosition = NavMesh.SamplePosition(goal, out hit, runningDistance / 5, agent.areaMask);
                if (samplePosition)
                {
                    agent.CalculatePath(hit.position, path);
                    yield return new WaitUntil(() => path.status != NavMeshPathStatus.PathInvalid);
                    agent.path = path;
                }
                isPathValid = (samplePosition && path.status != NavMeshPathStatus.PathPartial && agent.remainingDistance <= runningDistance);
                if (index > 13)
                { 
                    ChangeState(NpcStates.Idle);
                    break;
                }
            } while (!isPathValid);
            if (timeToRun < 2f)
                agent.acceleration = currentAcceleration;
            yield return new WaitUntil(() => Vector3.Distance(agent.destination, transform.position) <= runningDistance / 1.2);
            if (interaction == 1)
                agent.acceleration = scaredAcceleration;
        }
        agent.acceleration = currentAcceleration;
        ChangeState(NpcStates.Idle);
    }

    IEnumerator RotateTo(GameObject target)
    {
        Quaternion lookRotation;
        do
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / (Quaternion.Angle(transform.rotation, lookRotation) / agent.angularSpeed));
            yield return new WaitForEndOfFrame();
        } while (currentState == NpcStates.Talking);
    }
}