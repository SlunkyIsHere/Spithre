using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")] 
    private PlayerMovement pm;
    [SerializeField] private Transform cam;
    [SerializeField] private Transform gunTip;
    [SerializeField] private LayerMask whatIsGrappleable;
    [SerializeField] private LineRenderer lr;

    [Header("Swinging")] 
    [SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] private float spring = 4.5f;
    [SerializeField] private float damper = 7f;
    [SerializeField] private float massScale = 4.5f;
    private SpringJoint joint;
    

    [Header("Grappling")] 
    [SerializeField] private float maxGrappleDistance;
    [SerializeField] private float grappleDelayTime;
    [SerializeField] private float overshootYAxis;
    private Vector3 grapplePoint;

    [Header("Cooldown")] 
    [SerializeField] private float grapplingCoolDown;
    private float grapplingCoolDownTimer;

    [Header("Input")] 
    [SerializeField] private KeyCode grappleKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse2;

    private bool isGrappling;

    private void StartGrapple()
    {
        if (grapplingCoolDownTimer > 0) return;

        isGrappling = true;

        pm.isFreezing = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        pm.isFreezing = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;
        
        pm.JumpToPosition(grapplePoint, highestPointOnArc);
        
        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        pm.isFreezing = false;
        
        isGrappling = false;

        grapplingCoolDownTimer = grapplingCoolDown;

        lr.enabled = false;
    }

    private void StartSwing()
    {
        
    }

    private void ExecuteSwing()
    {
        
    }

    public void StopSwing()
    {
        
    }
    
    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if (Input.GetKeyDown(swingKey)) StartSwing();

        if (grapplingCoolDownTimer > 0)
            grapplingCoolDownTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (isGrappling)
        {
            lr.SetPosition(0, gunTip.position);
        }
    }
}
