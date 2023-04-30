using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AttackRadius : MonoBehaviour
{
    public SphereCollider Collider;
    protected List<IDamagable> Damagables = new List<IDamagable>();
    public int Damage = 10;
    public float AttackDelay;
    public delegate void AttackEvent(IDamagable Target);
    public AttackEvent OnAttack;
    protected Coroutine _attackCoroutine;
    public EnemyMovement movement;

    protected virtual void Awake()
    {
        Collider = GetComponent<SphereCollider>();
        //Collider.isTrigger = true;
    }
    
    protected virtual void OnTriggerEnter(Collider other)
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

    protected virtual void OnTriggerExit(Collider other)
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
    
    protected virtual IEnumerator Attack()
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
                yield return wait;
                closestDamagable.TakeDamage(Damage);
            }

            closestDamagable = null;
            closestDistance = float.MaxValue;

            yield return wait;

            Damagables.RemoveAll(DisabledDamagaables);
        }
        
        _attackCoroutine = null;
    }
    
    protected bool DisabledDamagaables(IDamagable damagable)
    {
        return damagable != null && !damagable.GetTransform().gameObject.activeSelf;
    }
}
