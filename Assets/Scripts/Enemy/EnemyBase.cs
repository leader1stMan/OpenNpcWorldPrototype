using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField]
    protected NavMeshAgent agent = null;

    public CharacterStats stats;
    /// <summary>
    /// This event has two parameters. 
    /// The first one is the old state and the other one is the new state
    /// </summary>
    public EnemyStateChangeEvent OnStateChanged;

    [Tooltip("The collider representing the area in which the enemy preffer to stay. " +
            "It can still be lured out of the area by npcs and the player. " +
            "This is an optional field")]
    public Collider PrefferedPatrolAreaCollider;

    #region Debugging
    public bool ShowDebugMessages;
    public bool VisualiseAgentActions;
    #endregion

    public float VisionRange;
    public LayerMask WhatCanThisEnemyAttack;
    public EnemyState CurrentState { get; private set; }

    Transform currentTarget;
    float attackCooldown;

    #region Editor Only

#if UNITY_EDITOR
    Transform DebugSphere;
#endif
    #endregion
    public virtual void Awake()
    {
        if (OnStateChanged == null)
            OnStateChanged = new EnemyStateChangeEvent();

        if (stats == null)
            stats = GetComponent<CharacterStats>();

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
    }
    protected virtual void Start()
    {
        SubscribeToEvents();
        PatrolToAnotherSpot();
    }

    protected virtual void Update()
    {
        ManageState();
        CheckForTargets();
        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        #region Editor Only
#if UNITY_EDITOR
        if (VisualiseAgentActions)
        {
            DebugSphere.position = agent.destination;
        }
#endif
        #endregion

    }

    public abstract void Attack(GameObject target);

    //This function is to be called from animation
    public abstract void DealDamage();

    protected virtual void ManageState()
    {
        if(CurrentState == EnemyState.Patroling)
        {
            if(Vector3.Distance(transform.position,agent.destination)<agent.stoppingDistance)
            {
                
                PatrolToAnotherSpot();
            }
        }
        else if(CurrentState == EnemyState.Chasing)
        {
            if (!currentTarget)
            {
                ChangeState(EnemyState.Idle);
                return;            
            }
            if ((currentTarget.position - transform.position).magnitude <= stats.GetWeapon().Range)
            {
                if (attackCooldown <= 0)
                {
                    Attack(currentTarget.gameObject);
                    ChangeState(EnemyState.Attacking);
                    attackCooldown = stats.GetWeapon().Cooldown; 
                } 
            }
            else
            {
                Chase(currentTarget);
            }
        }
        else if (CurrentState == EnemyState.Attacking)
        {
            if (!currentTarget)
            {
                ChangeState(EnemyState.Idle);
                return;
            }
            if (attackCooldown <= 0)
            {
                if ((currentTarget.position - transform.position).magnitude <= stats.GetWeapon().Range)
                {
                    Attack(currentTarget.gameObject);
                    attackCooldown = stats.GetWeapon().Cooldown;
                }
                else  ChangeState(EnemyState.Chasing);

            }
        }
        else if (CurrentState == EnemyState.Idle)
        {
            PatrolToAnotherSpot();
            ChangeState(EnemyState.Patroling);
        }
        
    }


    protected virtual void CheckForTargets()
    {
        if (currentTarget == null)
        {
            if (Physics.CheckSphere(transform.position, VisionRange, WhatCanThisEnemyAttack))
            {
                Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, WhatCanThisEnemyAttack);
                foreach (Collider col in cols)
                {
                   
                    RaycastHit hit;
                    if (VisualiseAgentActions)
                        Debug.DrawRay(transform.position, (col.transform.position - transform.position).normalized * VisionRange, Color.red);
                    if (Physics.Raycast(transform.position, (col.transform.position - transform.position).normalized,out hit, VisionRange))
                    {
                        if (hit.transform == this.transform)
                            continue;
                        if (hit.transform == col.transform)
                        {
                            currentTarget = col.transform;
                            break;
                        }
                    }
                    else
                    {
                        currentTarget = null;
                    }
                }
                
            }
            if (currentTarget != null)
            {
                Chase(currentTarget);
            }
            else
            {
                if (CurrentState == EnemyState.Attacking || CurrentState == EnemyState.Chasing)
                    ChangeState(EnemyState.Idle);
            }


        }
        else
        {
            RaycastHit hit;
            if(VisualiseAgentActions)
            Debug.DrawRay(transform.position, (currentTarget.position - transform.position).normalized * VisionRange, Color.red);
            if (Physics.Raycast(transform.position, (currentTarget.position - transform.position).normalized, out hit,VisionRange))
            {
                
                
                if (hit.transform != currentTarget.transform || Vector3.Distance(transform.position,hit.transform.position)>VisionRange)
                {

                    currentTarget = null;
                    ChangeState(EnemyState.Idle);
                }
            }
        }
    }
    private void Chase(Transform target)
    {
        currentTarget = target;
        agent.SetDestination(target.position);
        ChangeState(EnemyState.Chasing);
    }

    protected virtual void ChangeState(EnemyState state)
    {
        if (CurrentState == state)
            return;
        OnStateChanged.Invoke(CurrentState, state);
        
        CurrentState = state;
    }

    private void SubscribeToEvents()
    {
        OnStateChanged.AddListener(ManageStateChange);
    }

    protected virtual void ManageStateChange(EnemyState oldState,EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.Attacking:
                if (ShowDebugMessages)
                    Debug.Log(transform.name + " is attacking " + currentTarget.name);
                break;
            case EnemyState.Chasing:
                if (ShowDebugMessages)
                    Debug.Log(transform.name+" is chasing " + currentTarget.name);
                break;
            case EnemyState.Idle:
                if (ShowDebugMessages)
                    Debug.Log(name + " is idle");
                break;
            case EnemyState.Patroling:
                if(ShowDebugMessages)
                        Debug.Log(name + " is patrolling");
                break;
        }
    }

    protected virtual void PatrolToAnotherSpot()
    {
        Vector3 dest = transform.position;
        if (PrefferedPatrolAreaCollider == null)
        {
            dest = new Vector3(
                Random.Range(transform.position.x - VisionRange*2,transform.position.x+VisionRange*2),
                (transform.position.y),     
                Random.Range(transform.position.z - VisionRange*2 , transform.position.z + VisionRange*2)
                );
        }
        else
        {
            dest = new Vector3(
                Random.Range(PrefferedPatrolAreaCollider.bounds.min.x, PrefferedPatrolAreaCollider.bounds.max.x),
                Random.Range(PrefferedPatrolAreaCollider.bounds.min.y, PrefferedPatrolAreaCollider.bounds.max.y),
                Random.Range(PrefferedPatrolAreaCollider.bounds.min.z, PrefferedPatrolAreaCollider.bounds.max.z)
                );
        }
        NavMeshHit hit;
        if(NavMesh.SamplePosition(dest, out hit, VisionRange, 1))
        {
            dest = hit.position;
            ChangeState(EnemyState.Patroling);
            agent.SetDestination(hit.position);

        }
        else
        {
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

    protected virtual void OnDestroy()
    {

        #region Debug
#if UNITY_EDITOR
        if (VisualiseAgentActions && DebugSphere != null)
        {
            Destroy(DebugSphere.gameObject);
            if (ShowDebugMessages)
                Debug.Log(this.gameObject.name + " is destroyed");
        }
#endif
        #endregion
    }
}
/// <summary>
/// This event has two parameters. The first one is the old state and the other is the new state
/// </summary>
public class EnemyStateChangeEvent : UnityEvent<EnemyState,EnemyState> { }