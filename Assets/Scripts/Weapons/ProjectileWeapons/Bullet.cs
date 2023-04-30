using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject explosion;
    [SerializeField] private LayerMask whatIsEnemies;

    [Range(0f, 1f)] 
    [SerializeField] private float bounciness;
    [SerializeField] private bool useGravity;

    [SerializeField] private int explosionDamage;
    [SerializeField] private float explosionRange;
    [SerializeField] private float explosionForce;

    [SerializeField] private int maxCollisions;
    [SerializeField] private float maxLifeTime;
    [SerializeField] private bool explodeOnTouch = true;

    private int collisions;
    private PhysicMaterial physocMaterial;

    void Start()
    {
        Setup();
    }

    void Update()
    {
        if (collisions > maxCollisions) Explode();

        maxLifeTime -= Time.deltaTime;
        if (maxLifeTime <= 0) Explode();
    }

    private void Explode()
    {
        if (explosion != null) Instantiate(explosion, transform.position, Quaternion.identity);

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].GetComponent<Enemy>().TakeDamage(explosionDamage);

            if (enemies[i].GetComponent<Rigidbody>())
            {
                enemies[i].GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRange);
            }
        }
        
        Invoke("Delay", 0.05f);
    }

    private void Delay()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Bullet")) return;

        collisions++;
        
        if (collision.collider.CompareTag("whatIsEnemy") && explodeOnTouch) Explode();
    }

    private void Setup()
    {
        physocMaterial = new PhysicMaterial();
        physocMaterial.bounciness = bounciness;
        physocMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        physocMaterial.bounceCombine = PhysicMaterialCombine.Maximum;

        GetComponent<SphereCollider>().material = physocMaterial;

        rb.useGravity = useGravity;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }
}
