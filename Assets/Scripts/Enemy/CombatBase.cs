using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public abstract class CombatBase : MonoBehaviour
{
    [SerializeField]
    public NavMeshAgent agent = null;

    public AnimationController controller;

    public CharacterStats stats;

    [Tooltip("The collider representing the area in which the enemy preffer to stay. " +
            "It can still be lured out of the area by npcs and the player. " +
            "This is an optional field")]
    public Collider PatrolArea;
    public Transform attackPoint; //Npc attacks enemies while going to area 

    #region Debugging
    public bool ShowDebugMessages;
    public bool VisualiseAgentActions;
    #endregion

    public float VisionRange;
    public LayerMask WhatCanThisEnemyAttack;
    [TagSelector] public List<string> Tags;
    public EnemyState CurrentState{get; private set;}

    public Transform currentTarget;
    public float CombatRange;
    protected float attackCooldown;
    protected float AttackDistance;

    protected bool attack = false;
    #region Editor Only

#if UNITY_EDITOR
    Transform DebugSphere;
#endif
    #endregion

    protected virtual void Start()
    {
        controller = GetComponentInChildren<AnimationController>();
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<CharacterStats>();
        AttackDistance = stats.weapon.Range;

        GetComponent<NPC>().DisableRagdoll();
        #region Editor Only
#if UNITY_EDITOR
        if (VisualiseAgentActions)
        {
            GameObject debugsphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(debugsphere.GetComponent<Collider>());
            debugsphere.name = "Target Debugger for " + transform.name;
            GameObject debugparent = GameObject.Find("Debugger");
            if (debugparent == null)
                debugparent = new GameObject("Debugger");
            debugsphere.transform.SetParent(debugparent.transform);
            DebugSphere = debugsphere.transform;
        }
#endif
        #endregion

        ChangeState(EnemyState.Idle);
    }

    protected virtual void Update()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime; //decreases attack cooldown\
            stats.attackCooldown = attackCooldown;
        }

        if (currentTarget != null)
        {
            if (currentTarget.gameObject.layer == 8)
            {
                if (currentTarget.GetComponent<NPC>().currentState == NpcStates.Dead)
                    currentTarget = null;
            }
        }
        ManageState();
        MoveAnimaton();

        if (agent.enabled)
        {
            if (agent.isStopped == true)
            {
                agent.enabled = false;
                GetComponent<NavMeshObstacle>().enabled = true;
            }
        }


        #region Editor Only
#if UNITY_EDITOR
        if (VisualiseAgentActions)
        {
            DebugSphere.position = agent.destination;
        }
#endif
        #endregion
    }

    protected virtual void MoveAnimaton()
    {
        //Manage animations
        if (agent.velocity.magnitude == 0 || agent.isStopped == true)
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

    //Checks which is the current state and makes the Ai do the chosen behaviours every Update
    protected virtual void ManageState() 
    {
        switch (CurrentState)
        {
            case EnemyState.Patroling:
                //Find new target and start chasing it, else patrol
                Transform target = CheckForTargets();
                if (target != null)
                {
                    currentTarget = target;
                    ChangeState(EnemyState.Chasing);
                    return;
                }
                Debug.Log(true);
                if (agent.remainingDistance == 0)
                {
                    ChangeState(EnemyState.Idle);
                }
                break;

            case EnemyState.Idle:
                if (!attackPoint)
                {
                    //If point is reached, patrol to another
                    if (agent.enabled)
                    {
                        if (agent.remainingDistance <= agent.stoppingDistance * 2)
                            PatrolToAnotherSpot();
                    }
                }
                else
                {
                    ChangeState(EnemyState.Patroling);
                }
                break;
                
            case EnemyState.Chasing:
                //If target is null, switch to idle state
                if (currentTarget == null)
                {
                    ChangeState(EnemyState.Idle);
                }
                else if ((currentTarget.position - transform.position).magnitude <= AttackDistance)
                {
                    agent.isStopped = true;
                    ChangeState(EnemyState.Attacking);
                }
                else
                {
                    Chase(currentTarget);
                }
                break;

            case EnemyState.Attacking:
                if (currentTarget == null)
                {
                    ChangeState(EnemyState.Idle);
                }
                else
                {
                    if ((currentTarget.position - transform.position).magnitude <= AttackDistance)
                    {
                        if (attack == false)
                        {
                            Attack(currentTarget.gameObject);
                        }
                    }
                    else
                    {
                        ChangeState(EnemyState.Chasing);
                    }
                }
                break;

            case EnemyState.Dead:
                break;
            default:
                break;
        }
    }

    protected virtual Transform CheckForTargets()
    {
        List<Collider> possibleTargets = new List<Collider>();

        //Return all attackable target colliders in sphere
        Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, WhatCanThisEnemyAttack);
        foreach (Collider col in cols)
        {
            if (VisualiseAgentActions)
                Debug.DrawRay(transform.position, (col.transform.position - transform.position).normalized * VisionRange, Color.red);

            //Make sure the collider is not owned by this Ai
            if (col.transform == this.transform)
                continue;

            //Check if collider has attackable tag
            for (int i = 0; i < Tags.Capacity; i++)
            {
                if (col.gameObject.tag == Tags[i])
                {
                    possibleTargets.Add(col);
                    break;
                }
            }
        }

        if (possibleTargets.Count > 0)
        {
            //Find the nearest target
            Collider nearestTarget = possibleTargets[0];
            for (int i = 1; i < possibleTargets.Count; i++)
            {
                if (Vector3.Distance(possibleTargets[i].transform.position, transform.position)
                    < Vector3.Distance(nearestTarget.transform.position, transform.position))
                    nearestTarget = possibleTargets[i];
            }

            CombatBase[] combatBases = GameObject.FindObjectsOfType<CombatBase>();
            for (int i = 0; i < combatBases.Length; i++)
            {
                if (GetComponent<CombatBase>() == combatBases[i] || combatBases[i].enabled == false)
                {
                    for (int a = i; a < combatBases.Length - 1; a++)
                    {
                        // moving elements downwards, to fill the gap at [index]
                        combatBases[a] = combatBases[a + 1];
                    }
                    System.Array.Resize(ref combatBases, combatBases.Length - 1);
                }
            }

            int howmanyTarget = 0;
            for (int i = 0; i < combatBases.Length; i++)
            {
               if (combatBases[i].currentTarget == nearestTarget.transform)
               {
                    howmanyTarget++;
               }
            }

            if (howmanyTarget < 1)
            {
                return nearestTarget.transform;
            }
            else
            {
                return null;
            }
        }
        else
            return null;
    }

    void Chase(Transform target)
    {
        if (currentTarget == null)
        {
            ChangeState(EnemyState.Idle);
            return;
        }
        
        if (agent.enabled)
        {
            if (agent.destination != currentTarget.position)
            {
                currentTarget = target;
                agent.SetDestination(target.position);
            }
        }
    }
    public abstract void Attack(GameObject target);

    protected virtual void ChangeState(EnemyState state)
    {
        if (CurrentState == state)
            return;
        ManageStateChange(CurrentState, state);

        CurrentState = state;
    }
    protected virtual void ManageStateChange(EnemyState oldState, EnemyState newState)
    {
        switch (oldState)
        {
            case EnemyState.Attacking:
                if (GetComponent<CharacterStats>().isBlocking)
                    GetComponent<CharacterStats>().isBlocking = false;

                GetComponent<NavMeshObstacle>().enabled = false;
                StartCoroutine(EnablenNavmeshAgain());
                break;
            default:
                break;
        }
        switch (newState)
        {
            case EnemyState.Attacking:
                #region Debug

                if (ShowDebugMessages)
                    Debug.Log(transform.name + " is attacking " + currentTarget.name); 
                #endregion
                break;
            case EnemyState.Chasing:
                #region Debug

                if (ShowDebugMessages)
                    Debug.Log(transform.name + " is chasing " + currentTarget.name); 
                #endregion
                break;
            case EnemyState.Idle:
                #region Debug

                if (ShowDebugMessages)
                    Debug.Log(name + " is idle"); 
                #endregion
                break;
            case EnemyState.Patroling:
                if (attackPoint)
                {
                    agent.SetDestination(attackPoint.position + new Vector3(Random.Range(-10, 10), attackPoint.position.y, Random.Range(-10, 10)));
                }
                #region Debug

                if (ShowDebugMessages)
                    Debug.Log(name + " is patrolling"); 
                #endregion
                break;
            case EnemyState.Blocking:
                #region Debug

                if (ShowDebugMessages)
                    Debug.Log(name + " is blocking"); 
                #endregion
                break;
            case EnemyState.Dead:
                break;
        }
    }

    //Pick random spot and start moving there
    protected virtual void PatrolToAnotherSpot()
    {
        if (!attackPoint)
        {
            Vector3 dest;
            if (PatrolArea == null)
            {
                //Pick spot within X4 VisionRange 
                dest = new Vector3(
                    Random.Range(transform.position.x - VisionRange * 2, transform.position.x + VisionRange * 2),
                    (transform.position.y),
                    Random.Range(transform.position.z - VisionRange * 2, transform.position.z + VisionRange * 2)
                    );
            }
            else
            {
                //Pick spot within Patrol Area collider
                dest = new Vector3(
                    Random.Range(PatrolArea.bounds.min.x, PatrolArea.bounds.max.x),
                    Random.Range(PatrolArea.bounds.min.y, PatrolArea.bounds.max.y),
                    Random.Range(PatrolArea.bounds.min.z, PatrolArea.bounds.max.z)
                    );
            }
            NavMeshHit hit;
            if (NavMesh.SamplePosition(dest, out hit, VisionRange, agent.areaMask))
            {
                ChangeState(EnemyState.Patroling);
                agent.SetDestination(hit.position);
            }
            else
            {
                //Repeat till reachable spot is found
                PatrolToAnotherSpot();
            }
        }
        else
        {
            ChangeState(EnemyState.Patroling);
            agent.SetDestination(attackPoint.position);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (VisualiseAgentActions)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, VisionRange);
        }
    }

    public CombatBase EnableCombat()
    {
        if (GetComponent<CharacterStats>().weapon.type == WeaponType.LowRange)
        {
            GetComponent<ShieldMeleeAI>().enabled = true;
            return GetComponent<ShieldMeleeAI>();
        }
        else
        {
            GetComponent<ArcherAI>().enabled = true;
            return GetComponent<ArcherAI>();
        }
    }

    public IEnumerator EnablenNavmeshAgain()
    {
        yield return 2;
        GetComponent<NavMeshAgent>().enabled = true;
        GetComponent<NavMeshAgent>().isStopped = false;
    }
}