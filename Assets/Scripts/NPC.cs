using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.IO;
using TMPro;

public class NPC : NpcData, IAttackable, IDestructible
{
    public bool ShowDebugMessages;
    
    private AnimationController controller;

    //Navigation
    public NavMeshAgent agent { get; private set; }
    public float movementSpeed;

    public GameObject Attacker;
    public bool isAttacked;
    public float scaredRunningSpeed;
    public float scaredAcceleration;
    public float runningDistance;
    public float runningTime;
    private float runTimeLeft;

    //NPC-NPC interaction
    public bool isFirst;
    string path = null;
    private TMP_Text text;
    public List<string> DialoguePaths;

    //Ragdoll
    Rigidbody[] rig;
    SkinnedMeshRenderer[] skin;

    public bool combatState = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponentInChildren<AnimationController>();
        text = GetComponentInChildren<TMP_Text>();

        FindObjectOfType<DayAndNightControl>().OnMorningHandler += GoToWork; //Connects with the day and night controller
        FindObjectOfType<DayAndNightControl>().OnEveningHandler += GoHome; //On a certain time these functions are called so npcs can execute life cycles  

        DisableRagdoll();
    }

    public void DisableRagdoll()
    {
        skin = GetComponentsInChildren<SkinnedMeshRenderer>();
        rig = GetComponentsInChildren<Rigidbody>();

        foreach (SkinnedMeshRenderer skinned in skin)
        {
            skinned.updateWhenOffscreen = false; //has to be enabled when ragdoll is in. Otherwise the character sometimes does not render
        }

        foreach (Rigidbody rigidbody in rig)
        {
            rigidbody.GetComponent<Collider>().enabled = false; //Make sure colliders for the ragdoll are disabled while npc is still alive
            rigidbody.isKinematic = true;
        }

        GetComponent<CapsuleCollider>().enabled = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    void Update()
    {
        //Decrease run time after hit
        if (runTimeLeft > 0)
        {
            runTimeLeft -= Time.deltaTime;
        }

        if (combatState)
        {
            this.enabled = false;
            if (GetComponent<CharacterStats>().weapon.type == WeaponType.LowRange)
            {
                GetComponent<ShieldMeleeAI>().enabled = true;
            }
            else
            {
                GetComponent<ArcherAI>().enabled = true;
            }
        }
        WatchEnvironment();
    }

    void FixedUpdate()
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
    }

    //Check environment to run if another npc is attacking or being attacked
    private void WatchEnvironment()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, VisionLayers);

        foreach (Collider col in cols)
        {
            // If the NPC is looking at another attacked NPC, run
            if (col.gameObject.GetComponent<NPC>())
            {
                NPC npc = col.gameObject.GetComponent<NPC>();
                if (npc.isAttacked)
                {
                    Attacker = npc.Attacker;
                    ChangeState(NpcStates.Scared);
                }
            }
            // If the NPC is looking at an enemy Attacking, run
            else if (col.gameObject.GetComponent<CombatBase>())
            {
                CombatBase enemy = col.gameObject.GetComponent<CombatBase>();

                if (enemy.CurrentState == EnemyState.Attacking)
                {
                    Attacker = col.gameObject;
                    ChangeState(NpcStates.Scared);
                }
            }
        }
    }

    //Change NPC's state
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
                GetComponent<NavMeshAgent>().isStopped = true;
                break;
            case NpcStates.Working:
                SetMoveTarget(work);
                break;
            case NpcStates.Dead: //Enables ragdoll
                GetComponent<ShieldMeleeAI>().enabled = false;
                GetComponent<ArcherAI>().enabled = false;
                GetComponent<CharacterStats>().isDead = true;
                foreach (SkinnedMeshRenderer skinned in GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    skinned.updateWhenOffscreen = true; //Stops character from disrendering
                }

                GetComponentInChildren<AnimationController>().enabled = false; //Have to turn it off before executing ragdoll
                GetComponentInChildren<AnimationController>().animator.enabled = false;
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<Rigidbody>().isKinematic = false;

                foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
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
        if (this.enabled == false)
            return;

        //States that are more prioritized than 'GoingToWork' state
        if (currentState == NpcStates.GoingToWork || currentState == NpcStates.Working || currentState == NpcStates.Talking || currentState == NpcStates.Scared)
            return;
        StartCoroutine("GoToWorkCoroutine");
    }

    //Set agent destination to work position, and change state to "Working" as it is reached
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
        if (this.enabled == false)
            return;
        //States that are more prioritized than 'GoingHome' state
        if (currentState == NpcStates.GoingHome || currentState == NpcStates.Talking || currentState == NpcStates.Scared)
            return;
        StartCoroutine("GoHomeCoroutine");
    }

    void StopGoingHome()
    {
        agent.ResetPath();
        StopCoroutine("GoHomeCoroutine");
    }

    //Set agent destination to home position, and change state to "Idle" as it is reached
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

    public IEnumerator Conversation(GameObject talker = null, string converPath = null, Component senderScript = null)
    {
        if (talker == null)
        {
            StreamReader reader;
            string line;
            List<string> lines = new List<string>();
            path = converPath;

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
            ChangeState(NpcStates.Talking);
            StartCoroutine("RotateTo", talker); //Look at talker

            StreamReader reader;
            NPC npc = talker.GetComponent<NPC>();
            string line;
            List<string> lines = new List<string>();

            // if NPC is first, it randomly chooses conversation and assigns it to the second NPC
            // if NPC is second, it waits till conversation is assigned to him by the first NPC
            if (isFirst)
            {
                Debug.Log(gameObject.name + 2);
                //Assigning the conversation to npc1, npc2
                if (converPath == null)
                {
                    path = AssignPath();
                }
                else
                {
                    path = converPath;
                }
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
                Debug.Log(gameObject.name);
                yield return new WaitUntil(() => path != null); //Wait till first NPC sends the conversation he chose
                reader = new StreamReader(path);
                while (reader.ReadLine() != "{}") ;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            GetComponentInChildren<TMP_Text>().text = null; //Ui for showing text
            yield return new WaitUntil(() => isFirst); //We now don't need 'isFirst' to tell who started the conversation
            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].StartsWith(" ")) //Displays sentece for 4 secs
                {
                    GetComponentInChildren<TMP_Text>().text = lines[i];
                    yield return new WaitForSeconds(4);
                    GetComponentInChildren<TMP_Text>().text = null;
                }
                isFirst = false; //Turns 'isFirst' to false while pturning on it for the talker
                npc.isFirst = true; //Indicating that it's talker's time to speak
                yield return new WaitUntil(() => isFirst);
            }
            npc.isFirst = true;
        }

        EndConversation(senderScript);
    }

    //Randomly choose a path from DialoguePaths 
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

    //Stops talking state and removes all behaviours from it
    public void EndConversation(Component senderScript = null) 
    {
        GetComponent<NavMeshAgent>().isStopped = false;
        StopCoroutine("Conversation");
        StopCoroutine("RotateTo");
        path = null;
        isFirst = false;
        GetComponentInChildren<TMP_Text>().text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();

        if (senderScript == null)
        {
            if (currentState == NpcStates.Talking)
                ChangeState(NpcStates.Idle);
        }
        else if (senderScript == GetComponent<TreasonQuest>())
        {
            GetComponent<TreasonQuest>().EndConversation();
        }
    }

    //Start NPC-NPC interaction with nearby NPCs with 
    void OnTriggerStay(Collider other) 
    {
        if (this.enabled == false)
            return;
        // States that are more prioritized
        if (currentState == NpcStates.Scared || currentState == NpcStates.Talking)
            return;

        if (!other.CompareTag("Npc"))
            return;

        NPC NPCscript = other.GetComponentInParent<NPC>();

        //Checks if the talker's state does not have a higher priority
        if (NPCscript.currentState == NpcStates.Scared || NPCscript.currentState == NpcStates.Talking)
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

    //Called when NPC is attacked
    public void OnAttack(GameObject attacker, Attack attack)
    {
        Attacker = attacker;
        StartCoroutine("Attacked");
    }

    //Method WatchEnvironment() uses "IsAttacked" boolean to check if NPC is attacked
    IEnumerator Attacked() 
    {
        isAttacked = true;
        yield return new WaitForSeconds(1f);
        isAttacked = false;
    }

    //Run from "attacker" in opposite direction
    IEnumerator Run(GameObject attacker)
    {
        float currentAcceleration = agent.acceleration;
        agent.speed = scaredRunningSpeed;
        runTimeLeft = runningTime;
        agent.ResetPath();

        //Agent gets running acceleration at the first iteration
        int iteration = 0;
        while (runTimeLeft > 0)
        {
            Vector3 goal;
            NavMeshHit hit;
            bool isPathValid;
            NavMeshPath path = new NavMeshPath();

            //Get the angle between "attacker" and NPC
            Vector3 distanceIn3D = attacker.transform.position - transform.position;
            float magnitude = new Vector2(distanceIn3D.x, distanceIn3D.z).magnitude;
            Vector2 distance = new Vector2(distanceIn3D.x / magnitude, distanceIn3D.z / magnitude);
            double angleX = Math.Acos(distance.x);
            double angleY = Math.Asin(distance.y);

            //Loop has iteration limit to avoid errors
            int index = 0;
            const int limit = 13;

            //Loop tries to find further point from "attacker" in boundaries of a circle of "runningDistance" radius
            do
            {
                //Rotate point in the circle by (PI / 6 * index)
                angleX += index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                angleY -= index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                distance = new Vector2((float)Math.Cos(angleX), (float)Math.Sin(angleY));
                goal = new Vector3(transform.position.x - distance.x * runningDistance, transform.position.y, transform.position.z - distance.y * runningDistance);

                //Check if NPC can reach this point
                bool samplePosition = NavMesh.SamplePosition(goal, out hit, runningDistance / 5, agent.areaMask);
                //Calculate path if the point is reachable
                if (samplePosition)
                {
                    agent.CalculatePath(hit.position, path);
                    yield return new WaitUntil(() => path.status != NavMeshPathStatus.PathInvalid);
                    agent.path = path;
                }

                isPathValid = (samplePosition && 
                               path.status != NavMeshPathStatus.PathPartial && 
                               agent.remainingDistance <= runningDistance);
                
                //Stop loop if it is impossible to find way after "limit" iterations
                if (++index > limit)
                { 
                    ChangeState(NpcStates.Idle);
                    break;
                }
            } while (!isPathValid);

            yield return new WaitUntil(() => Vector3.Distance(agent.destination, transform.position) <= runningDistance / 1.2);
            
            if (++iteration == 1)
                agent.acceleration = scaredAcceleration;

            //Return to the default acceleration
            if (runTimeLeft < 2f)
                agent.acceleration = currentAcceleration;
        }
        agent.acceleration = currentAcceleration;
        ChangeState(NpcStates.Idle);
    }

    //Rotate to the target
    IEnumerator RotateTo(GameObject target)
    {
        Quaternion lookRotation;
        do
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / (Quaternion.Angle(transform.rotation, lookRotation) / GetComponent<NavMeshAgent>().angularSpeed));
            yield return new WaitForEndOfFrame();
        } while (currentState == NpcStates.Talking);
    }

    public void OnDestruction(GameObject destroyer)
    {
        ChangeState(NpcStates.Dead);
    }
}