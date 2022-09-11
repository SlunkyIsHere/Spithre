using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Grappling _grappling;
    [SerializeField] private PlayerMovement pm;

    [Header("Settings")]
    [SerializeField] private int quality = 200;
    [SerializeField] private float damper = 14;
    [SerializeField] private float strength = 800;
    [SerializeField] private float velocity = 15;
    [SerializeField] private float waveCount = 3;
    [SerializeField] private float waveHeight = 1;
    [SerializeField] private AnimationCurve affectCurve;

    private Spring _spring;
    private LineRenderer lr;
    private Vector3 currentGrapplePosition;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        _spring = new Spring();
        _spring.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        if (!_grappling.IsGrappling())
        {
            currentGrapplePosition = _grappling.gunTip.position;

            _spring.Reset();

            if (lr.positionCount > 0)
            {
                lr.positionCount = 0;
            }
            
            return;
        }

        if (lr.positionCount == 0)
        {
            _spring.SetVelocity(velocity);

            lr.positionCount = quality - 1;
        }

        _spring.SetDamper(damper);
        _spring.SetStrength(strength);
        _spring.MyUpdate(Time.deltaTime);

        Vector3 grapplePoint = _grappling.GetGrapplePoint();
        Vector3 gunTipPosition = _grappling.gunTip.position;

        Vector3 up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        for (int i = 0; i < quality; i++)
        {
            float delta = i / (float) quality;

            Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * _spring.Value *
                             affectCurve.Evaluate(delta);
            
            lr.SetPosition(i, Vector3.Lerp(gunTipPosition, currentGrapplePosition, delta) + offset);
        }
    }
}
