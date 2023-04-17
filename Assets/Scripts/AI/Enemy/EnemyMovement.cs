using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(AgentLinkMover))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] public Transform Target;
    [SerializeField] public float UpdateSpeed = 0.1f; // How often to update the path
    [SerializeField] private Animator animator;
    
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
    }

    public void StartChasing()
    {
        if (_followTargetCoroutine == null)
            _followTargetCoroutine = StartCoroutine(FollowTarget());
        else
            Debug.LogWarning("Already chasing");
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
    
    private IEnumerator FollowTarget()
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateSpeed);
        
        while (enabled)
        {
            Agent.SetDestination(Target.transform.position);
            
            yield return Wait;
        }
    }
    
    private void Update()
    {
        if (animator != null)
            animator.SetBool(IsWalking, Agent.velocity.magnitude > 0.1f);
    }
}
