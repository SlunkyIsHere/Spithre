using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaycastGun : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float impactForce = 30f;
    [SerializeField] private float fireRate = 15f;

    [SerializeField] private int magazineSize = 10;
    private int bulletsLeft;
    [SerializeField] private float reloadTime = 1f;
    private bool _isReloading;

    [SerializeField] private Camera cam;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject impactEffect;
    
    [SerializeField] private TextMeshProUGUI ammunitionDisplay;

    
    [HideInInspector]
    public bool isReadyToShoot = true;

    private float _nextTimeToFire = 0f;

    [SerializeField] private Animator animator;
    private static readonly int Reloading = Animator.StringToHash("Reloading");

    private void Start()
    {
        bulletsLeft = magazineSize;
    }

    private void OnEnable()
    {
        _isReloading = false;
        animator.SetBool(Reloading, false);
    }

    void Update()
    {
        if (_isReloading)
            return;

        if (bulletsLeft <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }
        
        if (Input.GetKey(KeyCode.Mouse0) && isReadyToShoot && Time.time >= _nextTimeToFire)
        {
            _nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
        
        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft + " / " + magazineSize);
    }

    IEnumerator Reload()
    {
        _isReloading = true;
        Debug.Log("Reloading...");
        
        animator.SetBool(Reloading, true);

        yield return new WaitForSeconds(reloadTime - .25f);
        
        animator.SetBool(Reloading, false);
        
        yield return new WaitForSeconds(.25f);


        bulletsLeft = magazineSize;
        _isReloading = false;
    }

    void Shoot()
    {
        muzzleFlash.Play();

        bulletsLeft--;
        
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range))
        {
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            GameObject impactGameObject = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactGameObject, 2f);
        }
    }
}
