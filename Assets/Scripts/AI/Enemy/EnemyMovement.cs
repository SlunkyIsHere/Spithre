using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(AgentLinkMover))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] public Transform Target;
    [SerializeField] public float UpdateSpeed = 0.1f; // How often to update the path
    [SerializeField] private Animator animator;
    //public NavMeshTriangulation Triangulation;

    public EnemyState DefaultState;
    private EnemyState _state;
    public bool ChaseOnStart = true;
    public EnemyLineOfSightChecker lineOfSightChecker;
    public EnemyState State
    {
        get
        {
            return _state;
        }
        set
        {
            OnStateChanged?.Invoke(_state, value);
            _state = value;
        }
    }
    
    public delegate void StateChangedEvent(EnemyState oldState, EnemyState newState);
    public StateChangedEvent OnStateChanged;
    public float IdleLocationRadius = 4f;
    public float IdleMoveSpeedModifier = 0.5f;
    public Vector3[] Waypoints = new Vector3[4];
    [SerializeField] private int WaypointIndex = 0;
    
    private AgentLinkMover _agentLinkMover;
    private NavMeshAgent Agent;
    
    private const string IsWalking = "IsWalking";
    private const string Jump = "Jump";
    private const string Landed = "Landed";
    
    private Coroutine _followTargetCoroutine;
    
    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();

        if (animator != null)
        {
            _agentLinkMover = GetComponent<AgentLinkMover>();
            _agentLinkMover.OnStartLink += HandleLinkStart;
            _agentLinkMover.OnEndLink += HandleLinkEnd;
        }
        
        lineOfSightChecker.OnGainSight += HandleGainSight;
        lineOfSightChecker.OnLoseSight += HandleLoseSight;
        
        OnStateChanged += HandleStateChanged;
        _state = DefaultState;
    }
    
    private void HandleGainSight(PlayerMovement playerMovement)
    {
        State = EnemyState.Chase;
        
    }
    
    private void HandleLoseSight(PlayerMovement playerMovement)
    {
        State = EnemyState.Patrol;
    }
    
    private void Start()
    {
        _state = DefaultState;
        OnStateChanged?.Invoke(EnemyState.Spawn, DefaultState);
    }

    private void HandleStateChanged(EnemyState oldstate, EnemyState newstate)
    {
        Debug.Log($"State changed from {oldstate} to {newstate}");

        if (oldstate != newstate)
        {
            if (_followTargetCoroutine != null)
            {
                StopCoroutine(_followTargetCoroutine);
            }

            if (oldstate == EnemyState.Idle)
            {
                Agent.speed /= IdleMoveSpeedModifier;
            }

            switch (newstate)
            {
                case EnemyState.Idle:
                    _followTargetCoroutine = StartCoroutine(DoIdleMotion());
                    break;
                case EnemyState.Patrol:
                    _followTargetCoroutine = StartCoroutine(DoPatrolMotion());
                    break;
                case EnemyState.Chase:
                    _followTargetCoroutine = StartCoroutine(FollowTarget());
                    break;
            }
        }
    }

    private IEnumerator DoPatrolMotion()
    {
        WaitForSeconds wait = new WaitForSeconds(UpdateSpeed);

        yield return new WaitUntil(() => Agent.enabled && Agent.isOnNavMesh);
        
        Agent.SetDestination(Waypoints[WaypointIndex]);

        while (true)
        {
            if (Agent.isOnNavMesh && Agent.enabled && Agent.remainingDistance <= Agent.stoppingDistance)
            {
                WaypointIndex++;

                if (WaypointIndex >= Waypoints.Length)
                {
                    WaypointIndex = 0;
                }
                
                Agent.SetDestination(Waypoints[WaypointIndex]);
            }

            yield return wait;
        }
    }

    private IEnumerator DoIdleMotion()
    {
        WaitForSeconds wait = new WaitForSeconds(UpdateSpeed);

        Agent.speed *= IdleMoveSpeedModifier;

        while (true)
        {
            if (!Agent.enabled || !Agent.isOnNavMesh)
            {
                yield return wait;
            } else if (Agent.remainingDistance <= Agent.stoppingDistance)
            {
                Vector2 point = Random.insideUnitCircle * IdleLocationRadius;
                NavMeshHit hit;

                if (NavMesh.SamplePosition(Agent.transform.position + new Vector3(point.x, 0, point.y), out hit, 2f, Agent.areaMask))
                {
                    Agent.SetDestination(hit.position);
                }
            }
            
            yield return wait;
        }
    }
    
    private IEnumerator FollowTarget()
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateSpeed);
        
        while (enabled)
        {
            if (Agent.enabled && Agent.isOnNavMesh) // Check if agent is enabled and on a NavMesh
            {
                Agent.SetDestination(Target.transform.position);
            }
            
            yield return Wait;
        }
    }

    public void StartChasing()
    {
        if (_followTargetCoroutine == null)
            _followTargetCoroutine = StartCoroutine(FollowTarget());
        else
            Debug.LogWarning("Already chasing");
    }

    private void OnDisable()
    {
        _state = DefaultState;
    }

    public void Spawn()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        for (int i = 0; i < Waypoints.Length; i++)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(triangulation.vertices[Random.Range(0, triangulation.vertices.Length)], out hit, 2f, Agent.areaMask))
            {
                Waypoints[i] = hit.position;
            }
            else
            {
                Debug.LogWarning("Failed to find a valid spawn point");
            }
        }
        OnStateChanged?.Invoke(EnemyState.Spawn, DefaultState);
    }

    private void HandleLinkStart()
    {
        animator.SetTrigger(Jump);
    }
    
    private void HandleLinkEnd()
    {
        animator.ResetTrigger(Jump);
        animator.SetTrigger(Landed);
    }
    
    public void StopChasing()
    {
        if (_followTargetCoroutine != null)
        {
            StopCoroutine(_followTargetCoroutine);
            _followTargetCoroutine = null;
        }
        else
            Debug.LogWarning("Not chasing");
    }
    
    private void Update()
    {
        if (animator != null)
            animator.SetBool(IsWalking, Agent.velocity.magnitude > 0.1f);
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < Waypoints.Length; i++)
        {
            Gizmos.DrawWireSphere(Waypoints[i], 0.25f);
            if (i + 1 < Waypoints.Length)
            {
                Gizmos.DrawLine(Waypoints[i], Waypoints[i + 1]);
            }
            else
            {
                Gizmos.DrawLine(Waypoints[i], Waypoints[0]);
            }
        }
    }
}
