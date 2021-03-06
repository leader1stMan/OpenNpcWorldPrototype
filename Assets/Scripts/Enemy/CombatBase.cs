﻿using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public abstract class CombatBase : MonoBehaviour
{
    [SerializeField]
    protected NavMeshAgent agent = null;
    public AnimationController controller;

    public CharacterStats stats;

    [Tooltip("The collider representing the area in which the enemy preffer to stay. " +
            "It can still be lured out of the area by npcs and the player. " +
            "This is an optional field")]
    public Collider PatrolArea;
    Rigidbody[] rig;
    SkinnedMeshRenderer[] skins;

    #region Debugging
    public bool ShowDebugMessages;
    public bool VisualiseAgentActions;
    #endregion

    public float VisionRange;
    public LayerMask WhatCanThisEnemyAttack;
    [TagSelector] public string[] Tags;
    public EnemyState CurrentState { get; set; }

    public Transform currentTarget;
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

        //Ragdoll effect. Currently this script and npc Ai script both has dead state stored separatly
        rig = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidbody in rig)
        {
            if (rigidbody != this.GetComponent<Rigidbody>())
            {
                rigidbody.GetComponent<Collider>().enabled = false;
                rigidbody.isKinematic = true;
            }
        }
        //Skinnedmesh needs to updated off screen when ragdolled. Or disappears
        skins = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skins)
        {
            skinnedMeshRenderer.updateWhenOffscreen = false;
        }


        GetComponent<CapsuleCollider>().enabled = true;

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
            if (currentTarget.tag == "Dead")
            {
                currentTarget = null;
            }
        }
        ManageState();

        #region Editor Only
#if UNITY_EDITOR
        if (VisualiseAgentActions)
        {
            DebugSphere.position = agent.destination;
        }
#endif
        #endregion
    }

    //Checks which is the current state and makes the Ai do the chosen behaviours every Update
    protected virtual void ManageState() 
    {
        switch (CurrentState)
        {
            case EnemyState.Patroling:
            case EnemyState.Idle:
                //Find new target and start chasing it, else patrol
                Transform target = CheckForTargets();
                if (target == null)
                {
                    //If point is reached, patrol to another
                    if (agent.remainingDistance <= agent.stoppingDistance * 2)
                        PatrolToAnotherSpot();
                }
                else
                {
                    currentTarget = target;
                    ChangeState(EnemyState.Chasing);
                }
                break;
                
            case EnemyState.Chasing:
                //If target is null, switch to idle state
                if (currentTarget == null)
                {
                    ChangeState(EnemyState.Idle);
                }
                else
                    if ((currentTarget.position - transform.position).magnitude <= AttackDistance)
                {
                    agent.isStopped = true;
                    ChangeState(EnemyState.Attacking);
                }
                else
                    Chase(currentTarget);
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
            for (int i = 0; i < Tags.Length; i++)
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
            return nearestTarget.transform;
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

        currentTarget = target;
        agent.SetDestination(target.position);
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
                agent.isStopped = false;
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

    protected virtual void OnDrawGizmosSelected()
    {
        if (VisualiseAgentActions)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, VisionRange);
        }
    }
}