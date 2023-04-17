using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AttackRadius : MonoBehaviour
{
    public SphereCollider Collider;
    private List<IDamagable> Damagables = new List<IDamagable>();
    public int Damage = 10;
    public float AttackDelay = 0.5f;
    public delegate void AttackEvent(IDamagable Target);
    public AttackEvent OnAttack;
    private Coroutine _attackCoroutine;
    public EnemyMovement movement;

    private void Awake()
    {
        Collider = GetComponent<SphereCollider>();
        Collider.isTrigger = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        IDamagable damagable = other.GetComponent<IDamagable>();
        if (damagable != null)
        {
            Debug.Log("Player entered attack radius");

            Damagables.Add(damagable);
            if (_attackCoroutine == null)
            {
                movement.StopChasing();
                _attackCoroutine = StartCoroutine(Attack());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IDamagable damagable = other.GetComponent<IDamagable>();
        if (damagable != null)
        {
            Damagables.Remove(damagable);
            if (Damagables.Count == 0)
            {
                movement.StartChasing();
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }
    }
    
    private IEnumerator Attack()
    {
        WaitForSeconds wait = new WaitForSeconds(AttackDelay);
        
        //yield return wait;

        IDamagable closestDamagable = null;
        float closestDistance = float.MaxValue;
        while (Damagables.Count > 0)
        {
            for (int i = 0; i < Damagables.Count; i++)
            {
                Transform damagableTransform = Damagables[i].GetTransform();
                float distance = Vector3.Distance(transform.position, damagableTransform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDamagable = Damagables[i];
                }
            }

            if (closestDamagable != null)
            {
                OnAttack?.Invoke(closestDamagable);
                closestDamagable.TakeDamage(Damage);
            }

            closestDamagable = null;
            closestDistance = float.MaxValue;

            yield return wait;

            Damagables.RemoveAll(DisabledDamagaables);
        }
        
        _attackCoroutine = null;
    }
    
    private bool DisabledDamagaables(IDamagable damagable)
    {
        return damagable != null && !damagable.GetTransform().gameObject.activeInHierarchy;
    }
}
