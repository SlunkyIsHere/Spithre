using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack Configuration", menuName = "ScriptableObject/Attack Configuration")]
public class AttackScriptableObject : ScriptableObject
{
    public bool isRanged = false;
    public int damage = 5;
    public float attackRadius = 1.5f;
    public float attackDelay = 1.5f;
    
    public EnemyBullet bulletPrefab;
    public Vector3 bulletSpawnOffset = new Vector3(0, 1, 0);
    public LayerMask LineOfSightLayers;

    public void SetupEnemy(Enemy enemy)
    {
        (enemy.attackRadius.Collider == null ? enemy.attackRadius.GetComponent<SphereCollider>() : enemy.attackRadius.Collider).radius = attackRadius;
        enemy.attackRadius.AttackDelay = attackDelay;
        enemy.attackRadius.Damage = damage;

        if (isRanged)
        {
            RangedAttackRadius rangedAttackRadius = enemy.attackRadius.GetComponent<RangedAttackRadius>();

            rangedAttackRadius.bulletPrefab = bulletPrefab;
            rangedAttackRadius.bulletSpawnOffset = bulletSpawnOffset;
            rangedAttackRadius.mask = LineOfSightLayers;
            
            rangedAttackRadius.CreateBulletPool();
        }
    }
}
