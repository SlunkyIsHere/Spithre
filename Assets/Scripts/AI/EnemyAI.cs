using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody rb;

    [SerializeField] private Transform player;
    [SerializeField] private LayerMask whatIsGround, whatIsPlayer;

    [SerializeField] private Vector3 walkPoint;
    private bool _walkPointSet;
    [SerializeField] private float walkPointRange;
    [SerializeField] private float health;

    [SerializeField] private float timeBetweenAttacks;
    private bool _alreadyAttacked;

    [SerializeField] private float sightRange, attackRange;
    [SerializeField] private bool isPlayerInSight, isPlayerInAttack;

    [SerializeField] private float jumpForce = 10;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        var position = transform.position;
        isPlayerInSight = Physics.CheckSphere(position, sightRange, whatIsPlayer);
        isPlayerInAttack = Physics.CheckSphere(position, attackRange, whatIsPlayer);
        
        if (!isPlayerInSight && !isPlayerInAttack) Patrolling();
        if (isPlayerInSight && !isPlayerInAttack) ChasePlayer();
        if (isPlayerInSight && isPlayerInAttack) AttackPlayer();
    }

    private void Patrolling()
    {
        if (!_walkPointSet) SearchWalkPoint();

        if (_walkPointSet) agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            _walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround)) _walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        
        transform.LookAt(player);

        if (!_alreadyAttacked)
        {
            Vector3 jumpDirection = (player.position - transform.position).normalized;
            rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);
            print("Jump Attacking");
            _alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        _alreadyAttacked = false;
    }

    //TODO: Add Player Damage
    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0) Invoke(nameof(DestroyEnemy), 2f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}
