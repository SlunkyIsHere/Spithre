using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabbing : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cam;
    [SerializeField] private Rigidbody rb;

    [Header("Ledge Grabbing")] 
    [SerializeField] private float moveToLedgeSpeed;
    [SerializeField] private float maxLedgeGrabDistance;
    [SerializeField] private float minTimeOnLedge;
    private float timeOnLedge;
    [SerializeField] public bool isHolding;

    [Header("Ledge Jumping")] 
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private float ledgeJumpForwardForce;
    [SerializeField] private float ledgeJumpUpwardForce;

    [Header("Ledge Detection")] 
    [SerializeField] private float ledgeDetectionLength;
    [SerializeField] private float ledgeSphereCastRadius;
    [SerializeField] private LayerMask whatIsLedge;

    private Transform lastLedge;
    private Transform currLedge;
    private RaycastHit ledgeHit;

    [Header("Exiting")] 
    [SerializeField] public bool isExitingLedge;
    [SerializeField] private float exitLedgeTime;
    private float exitLedgeTimer;

    private void SubStateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        bool anyInputKeyPressed = horizontalInput != 0 || verticalInput != 0;

        if (isHolding)
        {
            FreezeRigidbodyOnLedge();

            timeOnLedge += Time.deltaTime;
            
            if (timeOnLedge > minTimeOnLedge) ExitLedgeHold();
            
            if (Input.GetKeyDown(jumpKey)) LedgeJump();
        }
        else if (isExitingLedge)
        {
            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else isExitingLedge = false;
        }
    }
    
    private void LedgeDetection()
    {
        bool ledgeDetection = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit,
            this.ledgeDetectionLength, whatIsLedge);
        if (!ledgeDetection) return;

        float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);

        if (ledgeHit.transform == lastLedge) return;

        if (distanceToLedge < maxLedgeGrabDistance && !isHolding) EnterLedgeHold();
    }

    private void LedgeJump()
    {
        ExitLedgeHold();
        
        Invoke(nameof(DelayedJumpForce), 0.05f);
    }

    private void DelayedJumpForce()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);        
    }

    private void EnterLedgeHold()
    {
        isHolding = true;

        pm.isUnlimited = true;
        pm.isRestricted = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    private void FreezeRigidbodyOnLedge()
    {
        rb.useGravity = false;

        Vector3 directionToLedge = currLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, currLedge.position);

        if (distanceToLedge > 1f)
        {
            if (rb.velocity.magnitude < moveToLedgeSpeed)
                rb.AddForce(directionToLedge.normalized * (moveToLedgeSpeed * 1000f * Time.deltaTime));
        }
        else
        {
            if (!pm.isFreezing) pm.isFreezing = true;
            if (pm.isUnlimited) pm.isUnlimited = false;
        }
        
        if (distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
    }

    private void ExitLedgeHold()
    {
        isExitingLedge = true;
        exitLedgeTimer = exitLedgeTime;
        
        isHolding = false;
        timeOnLedge = 0f;
        
        pm.isRestricted = false;
        pm.isFreezing = false;

        rb.useGravity = true;
        
        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }

    private void Update()
    {
        LedgeDetection();
        SubStateMachine();
    }
}
