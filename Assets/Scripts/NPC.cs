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
    private AnimationController controller;

    public GameObject Attacker;
    public bool isAttacked;
    public float movementSpeed;
    public float scaredRunningSpeed;
    public float scaredAcceleration;
    public float runningDistance;
    public float runningTime;
    private float timeToRun;
    [SerializeField] private float speedAnimDevider = 1;

    private bool isFirst; //for the interaction between two npcs
    string path = null;
    private TMP_Text text;

    public List<string> DialoguePaths;

    Rigidbody[] rig; //for the ragdoll effect
    SkinnedMeshRenderer[] skin;

    public bool combatState; //just a debugging feature to check if the npc can turn into combat state

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        animator = GetComponentInChildren<Animator>();
        controller = GetComponentInChildren<AnimationController>();

        /*____________________Might need to change this as this is called every frame, interfering with the code______________________*/
        FindObjectOfType<DayAndNightControl>().OnMorningHandler += GoToWork; //Connects with the day and night controller
        FindObjectOfType<DayAndNightControl>().OnEveningHandler += GoHome; //On a certain time these functions are called so npcs can execute life cycles  

        text = GetComponentInChildren<TMP_Text>();
        
        //For ragdoll effect
        skin = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinned in skin) 
        {
            skinned.updateWhenOffscreen = false; //has to be enabled when ragdoll is in. Otherwise the character sometimes does not render
        }

        rig = GetComponentsInChildren<Rigidbody>(); 
        foreach (Rigidbody rigidbody in rig)
        {
            if (rigidbody != this.GetComponent<Rigidbody>())
            {
                rigidbody.GetComponent<Collider>().enabled = false; //Make sure colliders for the ragdoll are disabled while npc is still alive
                rigidbody.isKinematic = true;
            }
        }
        GetComponent<CapsuleCollider>().enabled = true; //Main collider for when the npc is alive
                                                        //We might not need it anymore(?) since the ragdoll colliders might work as well(Dunno)

        combatState = false; //We don't want combat state as the default state
        GetComponent<EnemyBase>().enabled = false;
    }
    void Update()
    {
        if (timeToRun > 0) //Npc fleeds if it senses danger as much as the amount given to 'timeToRun'
        {
            timeToRun -= Time.deltaTime;
        }

        if (combatState) 
            ChangeState(NpcStates.Combat);
        WatchEnvironment(); //Senses danger

        if (GetComponent<CharacterStats>().isDead)
        {
            ChangeState(NpcStates.Dead);
        }
    }

    void FixedUpdate()
    {
        if (currentState != NpcStates.Combat)
        {
            if (agent.velocity.magnitude == 0) //Meaning if gameobject has reached it's destination
            {
                controller.ChangeAnimation(AnimationController.IDLE, AnimatorLayers.ALL); //Changes the 'pose' of a character. Each animation in our game is a pose instead of an animation
            }
            else
            {
                if (agent.velocity.magnitude < 2.5f) //Walks or runs depending on the length left for reaching destination
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
            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, VisionLayers); //Returns all the colliders in a given range

            foreach (Collider col in cols)
            {
                // If the NPC is looking at another NPC attacking or defending, run
                if (col.gameObject.GetComponent<NPC>())
                {
                    NPC npc = col.gameObject.GetComponent<NPC>();
                    if (npc.isAttacked)
                    {
                        Attacker = npc.Attacker;
                        ChangeState(NpcStates.Scared); //ChangeState() changes the state of the npc. Each state has it's own behaviours
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

    private void ChangeState(NpcStates NewState) //Changes the state
    {
        if (currentState == NewState) //No need to change state
            return;

        NpcStates PrevState = currentState; //If state needs to be changed current state is now prev state

        currentState = NewState; 
        OnStateChanged(PrevState, NewState); //Make sure npc has stopped the behaviours from the previous state. Enable 'some' of the behaviours for the current state
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
                GetComponent<EnemyBase>().enabled = false; //Combat has it's own script unlike other states
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
            case NpcStates.Dead: //Enables ragdoll
                foreach (SkinnedMeshRenderer skinned in skin)
                {
                    skinned.updateWhenOffscreen = true; //Stops character from disrendering
                }

                GetComponentInChildren<Animator>().enabled = false; //Have to turn off these before executing ragdoll
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
        //States that are more prioritized than 'GoingToWork' state
        if (currentState == NpcStates.GoingToWork || currentState == NpcStates.Working || currentState == NpcStates.Talking || currentState == NpcStates.Scared || currentState == NpcStates.Combat)
            return;
        StartCoroutine("GoToWorkCoroutine");
    }
    IEnumerator GoToWorkCoroutine() //Functions 'GoingToWorkState'
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
        //States that are more prioritized than 'GoingHome' state
        if (currentState == NpcStates.GoingHome || currentState == NpcStates.Talking || currentState == NpcStates.Scared || currentState == NpcStates.Combat)
            return;
        StartCoroutine("GoHomeCoroutine");
    }

    void StopGoingHome()
    {
        agent.ResetPath();
        StopCoroutine("GoHomeCoroutine");
    }

    IEnumerator GoHomeCoroutine() //Functions 'GoingHome'
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
        StartCoroutine("RotateTo", talker); //Look at talker

        StreamReader reader;
        NPC npc = talker.GetComponent<NPC>(); //Talker's npc script
        string line;
        List<string> lines = new List<string>(); //For storing the sent conversation
        if (isFirst) //Am I starting the conversation?
        {
            path = AssignPath(); //Assigning the conversation to npc1, npc2
            npc.path = path;
            reader = new StreamReader(path);
            while ((line = reader.ReadLine()) != "{}") //Every converstion is sotred in one file. What separates these conversations is the {}
            {
                lines.Add(line); //Storing the conversation by each line
            }
        }
        else
        {
            yield return new WaitUntil(() => path != null); //Wait till talker sends the conversation he chose
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
        EndConversation();
    }

    string AssignPath() //Choose which conversation
    {
        if (DialoguePaths.Count > 0)
            return DialoguePaths[UnityEngine.Random.Range(0, DialoguePaths.Count - 1)]; //Choose which converstion to speak
        else                                                                           
            return null;
    }
    public void StartConversation(GameObject talker)
    {
        StartCoroutine("Conversation", talker); //Ienumerator
    }
    public void EndConversation() //Stops talking state and removes all behaviours from it
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
    void OnTriggerStay(Collider other) //Triggers by near colliders every frame. Used for triggering npc conversation
    {
        // States that are more prioritized
        if (currentState == NpcStates.Scared || currentState == NpcStates.Talking || currentState == NpcStates.Combat)
            return;
        if (!other.CompareTag("Npc")) //if the collider is not a npc's
            return;
        NPC NPCscript = other.GetComponentInParent<NPC>();
        //Checks if the talker's state does not have a higher priority
        if (NPCscript.currentState == NpcStates.Scared || NPCscript.currentState == NpcStates.Talking || NPCscript.currentState == NpcStates.Combat)
            return;
        if (UnityEngine.Random.Range(0, 1000) <= 0) //At a chance starts a conversation
        {
            if (GetInstanceID() > NPCscript.GetInstanceID()) //Each script has it's own ID. We can use these so one of the npc scripts is more prioritized
            {                                                //Can stop bug for when both scripts decide to have a conversation at the same time                                                                                                     
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

    IEnumerator Attacked() //Sensing attack checks every update. So we want to use 'IsAttacked' so it will only be called once per danger
    {
        isAttacked = true;
        yield return new WaitForSeconds(1f);
        isAttacked = false;
    }
    IEnumerator Run(GameObject attacker) //When npc senses danger. Can you write comments for this Sans?
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

    IEnumerator RotateTo(GameObject target) //Look at target
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