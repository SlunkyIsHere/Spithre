using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform orientation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private LedgeGrabbing lg;
    [SerializeField] private LayerMask whatIsWall;

    [Header("Climbing")] 
    [SerializeField] private float climbSpeed;
    [SerializeField] private float maxClimbTime;
    private float climbTimer;
    private bool isClimbing;

    [Header("ClimbJumping")] 
    [SerializeField] private float climbJumpUpForce;
    [SerializeField] private float climbJumbBackForce;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    [SerializeField] private float detectionLength;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool isWallInfront;
    
    private Transform lastWall;
    private Vector3 lastWallNormal;
    [SerializeField] private float minWallNormalAngleChange;

    [Header("Exiting")]
    [SerializeField] public bool isExitingWall;
    [SerializeField] private float exitWalTime;
    private float exitWallTimer;

    private void StateMachine()
    {
        if (lg.isHolding)
        {
            if (isClimbing) StopClimbing();
        } else if (isWallInfront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !isExitingWall)
        {
            if (!isClimbing && climbTimer > 0) StartClimbing();

            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            
            if (climbTimer < 0) StopClimbing();
        } else if (isExitingWall)
        {
            if (isClimbing) StopClimbing();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) isExitingWall = false;
        }
        else
        {
            if (isClimbing) StopClimbing();
        }
        
        if(isWallInfront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) ClimbJump();
    }
    
    private void WallCheck()
    {
        isWallInfront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit,
            detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall ||
                       Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((isWallInfront && newWall) || pm.isGrounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;
        pm.isClimbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    private void StopClimbing()
    {
        isClimbing = false;
        pm.isClimbing = false;
    }

    private void ClimbJump()
    {
        if (pm.isGrounded) return;
        if (lg.isHolding || lg.isExitingLedge) return;

        isExitingWall = true;
        exitWallTimer = exitWalTime;
        
        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumbBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }

    private void Start()
    {
        lg = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        WallCheck();
        StateMachine();
        
        if (isClimbing && !isExitingWall) ClimbingMovement();
    }
}
