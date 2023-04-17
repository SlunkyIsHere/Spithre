using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : PoolableObject, IDamagable
{
   public AttackRadius attackRadius;
   public EnemyMovement movement;
   public NavMeshAgent agent;
   public Animator animator;
   public float health = 100;
   public bool isPooled = false;
   public EnemyScriptableObject EnemyScriptableObject;
   
   private Coroutine LookCoroutine;
   private const string ATTACK_TRIGGER = "Attack";

   private void Awake()
   {
      agent = GetComponent<NavMeshAgent>();
      attackRadius.OnAttack += OnAttack;
   }
   
   private void OnAttack(IDamagable target)
   {
      if (animator != null)
         animator.SetTrigger(ATTACK_TRIGGER);
      
      if (LookCoroutine != null)
         StopCoroutine(LookCoroutine);
      
      LookCoroutine = StartCoroutine(LookAt(target.GetTransform()));
   }
   
   private IEnumerator LookAt(Transform target)
   {
      Quaternion lookRotation = Quaternion.LookRotation(target.position - transform.position);
      
      float time = 0;

      while (time < 1)
      {
         transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
         time += Time.deltaTime * 2;
         yield return null;
      }
      
      transform.rotation = lookRotation;
   }

   private void Start()
   {
      if (!isPooled)
         movement.StartChasing();
   }

   public void OnEnable()
   {
      SetupAgentFromConfiguration();
   }

   public override void OnDisable()
   {
      base.OnDisable();

      if (agent != null && isPooled)
         agent.enabled = false;
   }

   public virtual void SetupAgentFromConfiguration()
   {
      agent.speed = EnemyScriptableObject.Speed;
      agent.angularSpeed = EnemyScriptableObject.AngularSpeed;
      agent.acceleration = EnemyScriptableObject.Acceleration;
      agent.areaMask = EnemyScriptableObject.AreaMask;
      agent.avoidancePriority = EnemyScriptableObject.AvoidancePriority;
      agent.stoppingDistance = EnemyScriptableObject.StoppingDistance;
      agent.radius = EnemyScriptableObject.Radius;
      agent.height = EnemyScriptableObject.Height;
      agent.baseOffset = EnemyScriptableObject.BaseOffset;
      agent.obstacleAvoidanceType = EnemyScriptableObject.ObstacleAvoidanceType;
      
      movement.UpdateSpeed = EnemyScriptableObject.AIUpdateInterval;
      
      health = EnemyScriptableObject.Health;
      
      attackRadius.Collider.radius = EnemyScriptableObject.AttackRadius;
      attackRadius.Damage = EnemyScriptableObject.Damage;
      attackRadius.AttackDelay = EnemyScriptableObject.AttackDelay;
   }

   public void TakeDamage(float damage)
   {
      health -= damage;
      if (health <= 0)
         gameObject.SetActive(false);
   }

   public Transform GetTransform()
   {
      return transform;
   }
}
