using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackRadius : AttackRadius
{
    public NavMeshAgent agent;
    public EnemyBullet bulletPrefab;
    public Vector3 bulletSpawnOffset = new Vector3(0, 1, 0);
    public LayerMask mask;
    private ObjectPool bulletPool;
    [SerializeField] private float sphereCastRadius = 0.1f;
    private RaycastHit hit;
    private IDamagable targetDamagable;
    private EnemyBullet bullet;

    private void Start()
    {
        if (agent == null)
        {
            Debug.LogError("RangedAttackRadius: NavMeshAgent not assigned.");
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("RangedAttackRadius: EnemyBullet prefab not assigned.");
        }

        CreateBulletPool();
    }
    
    public void CreateBulletPool()
    {
        if (bulletPool == null)
            bulletPool = ObjectPool.CreateInstance(bulletPrefab, Mathf.CeilToInt((1 / AttackDelay) * bulletPrefab.AutoDestroyTime));
    }

    protected override IEnumerator Attack()
    {
        WaitForSeconds wait = new WaitForSeconds(AttackDelay);

        yield return wait;

        while (Damagables.Count > 0)
        {
            for (int i = 0; i < Damagables.Count; i++)
            {
                if (HasLineOfSightTo(Damagables[i].GetTransform()))
                {
                    targetDamagable = Damagables[i];
                    OnAttack?.Invoke(Damagables[i]);
                    agent.enabled = false;
                    break;
                }
            }

            if (targetDamagable != null)
            {
                PoolableObject poolableObject = bulletPool.GetObject();
    
                if (poolableObject != null)
                {
                    bullet = poolableObject.GetComponent<EnemyBullet>();

                    bullet.transform.position = transform.position + bulletSpawnOffset;
                    bullet.transform.rotation = agent.transform.rotation;

                    bullet.Spawn(agent.transform.forward, Damage, targetDamagable.GetTransform());
                }
                else
                {
                    // Handle the case when poolableObject is null, e.g., skip this iteration or wait
                }
            }
            else
            {
                agent.enabled = true;
            }

            yield return wait;

            if (targetDamagable == null || !HasLineOfSightTo(targetDamagable.GetTransform()))
            {
                agent.enabled = true;
            }

            Damagables.RemoveAll(DisabledDamagaables);
        }

        agent.enabled = true;
        _attackCoroutine = null;
    }
    
    private bool HasLineOfSightTo(Transform target)
    {
        if (Physics.SphereCast(transform.position + bulletSpawnOffset, sphereCastRadius, 
                ((target.position + bulletSpawnOffset) - (transform.position + bulletSpawnOffset)).normalized, out hit, Collider.radius, mask))
        {
            IDamagable damagable;
            if (hit.collider.TryGetComponent<IDamagable>(out damagable))
            {
                return damagable.GetTransform() == target;
            }
            //return hit.collider.GetComponent<IDamagable>() != null;
        }

        return false;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (_attackCoroutine == null)
        {
            agent.enabled = true;
        }
    }
}
