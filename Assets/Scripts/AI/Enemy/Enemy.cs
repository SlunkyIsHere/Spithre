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
   
   private Coroutine LookCoroutine;
   private const string ATTACK_TRIGGER = "Attack";

   private void Awake()
   {
      //agent = GetComponent<NavMeshAgent>();
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
      while (true)
      {
         if (movement.State == EnemyState.Chase)
         {
            Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);

            Quaternion lookRotation = Quaternion.LookRotation(targetPosition - transform.position);

            float time = 0;

            while (time < 1)
            {
               transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
               time += Time.deltaTime * 2;
               yield return null;
            }

            transform.rotation = lookRotation;
         }

         yield return new WaitForSeconds(0.1f);
      }
   }

   private void Start()
   {
      movement.Spawn();

      if (!isPooled && movement.ChaseOnStart)
         movement.StartChasing();
   }

   public override void OnDisable()
   {
      base.OnDisable();

      if (agent != null && isPooled)
         agent.enabled = false;
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
