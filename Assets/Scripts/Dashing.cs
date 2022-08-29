using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform orientaion;
    [SerializeField] private Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Dashing")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashUpwardForce;
    [SerializeField] private float maxDashYSpeed;
    [SerializeField] private float dashDuration;

    [Header("CameraEffects")] 
    [SerializeField] private MainCamera cam;
    [SerializeField] private float dashFov;

    [Header("Settings")]
    [SerializeField] private bool useCameraForward = true;
    [SerializeField] private bool allowAllDirection = true;
    [SerializeField] private bool disableGravity = false;
    [SerializeField] private bool resetVel = true;

    [Header("Cooldown")] 
    [SerializeField] private float dashCoolDown;
    private float dashCoolDowntimer;

    [Header("Input")]
    [SerializeField] private KeyCode dashKey = KeyCode.E;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Dash()
    {
        if (dashCoolDowntimer > 0) 
            return;
        dashCoolDowntimer = dashCoolDown;

        pm.isDashing = true;
        pm.maxYSpeed = maxDashYSpeed;
        
        cam.DoFov(dashFov);

        Transform forwardT;

        if (useCameraForward)
            forwardT = playerCam;
        else
            forwardT = orientaion;

        Vector3 direction = GetDirection(forwardT);

        Vector3 forceToApply = direction * dashForce + orientaion.up * dashUpwardForce;

        if (disableGravity)
            rb.useGravity = false;

        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f);
        
        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    
    private void DelayedDashForce()
    {
        if (resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.isDashing = false;
        pm.maxYSpeed = 0;
        
        cam.DoFov(80f);

        if (disableGravity)
            rb.useGravity = true;
    }

    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3();

        if (allowAllDirection)
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        else
            direction = forwardT.forward;

        if (verticalInput == 0 && horizontalInput == 0)
            direction = forwardT.forward;

        return direction.normalized;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
        {
            Dash();
        }

        if (dashCoolDowntimer > 0)
            dashCoolDowntimer -= Time.deltaTime;
    }
}
