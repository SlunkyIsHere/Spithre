using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyBullet : PoolableObject
{
    public float AutoDestroyTime = 5f;
    public float Speed = 2f;
    public int Damage = 5;
    public Rigidbody Rigidbody;
    protected Transform Target;
    
    protected const string DISABLE_METHOD_NAME = "Disable";

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void OnEnable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Invoke(DISABLE_METHOD_NAME, AutoDestroyTime);
    }

    public virtual void Spawn(Vector3 forward, int damage, Transform target)
    {
        this.Damage = Damage;
        this.Target = target;
        Rigidbody.AddForce(forward * Speed, ForceMode.VelocityChange);
        
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        IDamagable damagable;
        
        if (other.TryGetComponent<IDamagable>(out damagable))
        {
            damagable.TakeDamage(Damage);
        }

        Disable();
    }

    protected virtual void Disable()
    {
        CancelInvoke(DISABLE_METHOD_NAME);
        Rigidbody.velocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
