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
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (collision.collider.CompareTag(""))
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
}
