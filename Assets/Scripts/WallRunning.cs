using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")] 
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpSideForce;
    [SerializeField] private float wallClimbSpeed;
    [SerializeField] private float maxWallRunTime;
    private float wallRunTimer;
     
    [Header("Input")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode upwardsRunKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool isRunningUpwards;
    private bool isRunningDownwards;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")] 
    private bool isExitingWall;
    [SerializeField] private float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")] 
    [SerializeField] private bool useGravity;
    [SerializeField] private float gravityCounterForce;

    [Header("References")] 
    [SerializeField] private Transform orientation;
    [SerializeField] private MainCamera cam;
    [SerializeField] private LedgeGrabbing lg;
    private PlayerMovement pm;
    private Rigidbody rb;

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance,
            whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance,
            whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        isRunningUpwards = Input.GetKey(upwardsRunKey);
        isRunningDownwards = Input.GetKey(downwardsRunKey);

        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !isExitingWall)
        {
            if (!pm.isWallRunning)
            {
                StartWallRun();
            }

            if (wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }

            if (wallRunTimer <= 0 && pm.isWallRunning)
            {
                isExitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Input.GetKeyDown(jumpKey))
            {
                WallJump();
            }
        } else if (isExitingWall)
        {
            if (pm.isWallRunning)
            {
                StopWallRun();
            }

            if (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            if (exitWallTimer <= 0)
            {
                isExitingWall = false;
            }
        }
        else 
        {
            if (pm.isWallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.isWallRunning = true;

        wallRunTimer = maxWallRunTime;
        
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        cam.DoFov(90f);
        if (wallLeft) cam.DoTilt(-5f);
        if (wallRight) cam.DoTilt(5f);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;
        
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (isRunningUpwards)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (isRunningDownwards)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
        
        if (!(wallLeft && horizontalInput > 0) || !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        
        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.isWallRunning = false;
        
        cam.DoFov(80f);
        cam.DoTilt(0f);
    }

    private void WallJump()
    {
        if (lg.isHolding || lg.isExitingLedge) return;
        
        
        isExitingWall = true;
        exitWallTimer = exitWallTime;
        
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        
        lg = GetComponent<LedgeGrabbing>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if(pm.isWallRunning)
            WallRunningMovement();
    }
}
