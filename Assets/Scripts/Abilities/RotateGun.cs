using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGun : MonoBehaviour
{
    [SerializeField] private Grappling grappling;

    private Quaternion _desiredRotation;
    private float _rotationSpeed = 1f;
    
    void Update()
    {
        if (!grappling.IsGrappling())
        {
            _desiredRotation = transform.parent.rotation * Quaternion.Euler(0, 90, 0);
        }
        else
        {
            _desiredRotation = Quaternion.LookRotation(grappling.GetGrapplePoint() - transform.position);
        }
        
        transform.rotation = Quaternion.Lerp(transform.rotation, _desiredRotation, Time.deltaTime * _rotationSpeed);
    }
}
