using UnityEngine;
using UnityEngine.AI;

public class SpiderSpeedManager : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform target;
    public float minAgentSpeed = 1f;
    public float maxAgentSpeed = 2f;
    public float slowDownDistance = 5f;

    private void Start()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        if (target != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            float currentAgentSpeed = Mathf.Lerp(minAgentSpeed, maxAgentSpeed, distanceToPlayer / slowDownDistance);
            agent.speed = currentAgentSpeed;
        }
    }
}