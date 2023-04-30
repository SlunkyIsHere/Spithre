using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyLineOfSightChecker : MonoBehaviour
{
    public new SphereCollider collider;
    public float fieldOfView = 90f;
    public LayerMask lineOfSightLayers;
    
    public delegate void GainSightEvent(PlayerMovement playerMovement);
    public GainSightEvent OnGainSight;
    public delegate void LoseSightEvent(PlayerMovement playerMovement);
    public LoseSightEvent OnLoseSight;
    
    private Coroutine _checkLineOfSightCoroutine;
    
    void Awake()
    {
        collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement;

        if (other.TryGetComponent<PlayerMovement>(out playerMovement))
        {
            if (!CheckLineOfSight(playerMovement))
            {
                _checkLineOfSightCoroutine = StartCoroutine(CheckForLineOfSight(playerMovement));
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        PlayerMovement playerMovement;

        if (other.TryGetComponent<PlayerMovement>(out playerMovement))
        {
            OnLoseSight?.Invoke(playerMovement);
            
            if (_checkLineOfSightCoroutine != null)
            {
                StopCoroutine(_checkLineOfSightCoroutine);
            }
        }
    }
    
    private bool CheckLineOfSight(PlayerMovement playerMovement)
    {
        Vector3 direction = (playerMovement.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(direction, transform.forward);

        if (Vector3.Dot(transform.forward, direction) >= Mathf.Cos(fieldOfView))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, collider.radius, lineOfSightLayers))
            {
                if (hit.transform.GetComponent<PlayerMovement>() != null)
                {
                    OnGainSight?.Invoke(playerMovement);
                    return true;
                }
            }
        }

        return false;
    }
    
    private IEnumerator CheckForLineOfSight(PlayerMovement playerMovement)
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        
        while (!CheckLineOfSight(playerMovement))
        {
            yield return wait;
        }
    }
}
