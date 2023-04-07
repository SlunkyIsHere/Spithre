 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    public float playerHeight = 2f;
    public Transform orientation; 
    
    [Header("Movement")]
    public float moveForce = 12f;
    public float airMultiplier = 0.4f;
    public float groundDrag = 5f;
    public float jumpForce = 13f;
    public float jumpCooldown = 0.25f;

    public float crouchYScale = 0.5f;
    private float startYScale;
    private bool crouchStarted;

    [Header("Special Movement")]
    public int doubleJumps = 1;
    private int doubleJumpsLeft;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode walkKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public bool readyToJump = true;

    [Header("Speed handling")]
    public float walkMaxSpeed = 4f;
    public float sprintMaxSpeed = 7f;
    public float crouchMaxSpeed = 2f;
    public float slopeSlideMaxSpeed = 30f;
    public float wallJumpMaxSpeed = 12f;
    public float climbMaxSpeed = 3f;
    public float dashMaxSpeed = 15f;
    public float swingMaxSpeed = 17f;
    public float airMaxSpeed = 7f;

    public float limitedMaxSpeed = 20f;

    public float dashSpeedChangeFactor;
    public float wallJumpSpeedChangeFactor;

    private float maxSpeed;
    private float desiredMaxSpeed;
    private float lastDesiredMaxSpeed;

    public float speedIncreaseMultiplier = 1.5f;
    public float slopeIncreaseMultiplier = 2.5f;
    
    [HideInInspector] public float maxYSpeed;

    [Header("Ground Detection")]
    public LayerMask whatIsGround;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float maxSlopeAngle = 40f;

    [Header("Jump Prediction")]
    public float maxJumpRange;
    public float maxJumpHeight;

    [Header("References")]
    private MainCamera cam;
    private WallRunning wr;

    [Header("Movement Modes")] 
    [HideInInspector] public MovementMode mm; 
    public enum MovementMode 
    {
        unlimited, 
        limited, 
        freeze, 
        dashing,
        sliding,
        crouching,
        sprinting,
        walking,
        wallrunning,
        walljumping,
        climbing,
        swinging,
        air
    };
    
    [HideInInspector] public bool freeze;
    [HideInInspector] public bool unlimitedSpeed;
    [HideInInspector] public bool restricted;
    [HideInInspector] private bool tierTwoRestricted;
    [HideInInspector] public bool dashing;
    [HideInInspector] public bool walking;
    [HideInInspector] public bool wallrunning;
    [HideInInspector] public bool walljumping;
    [HideInInspector] public bool climbing;
    [HideInInspector] public bool crouching;
    [HideInInspector] public bool sliding;
    [HideInInspector] public bool swinging;
    [HideInInspector] public bool jumping;

    [HideInInspector] private bool limitedSpeed;
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;
    [HideInInspector] public bool grounded;

    private Vector3 moveDirection;
    private Rigidbody rb;
    RaycastHit slopeHit;

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_ySpeed;
    public TextMeshProUGUI text_moveState;

    private void Start()
    {
        if (whatIsGround.value == 0)
            whatIsGround = LayerMask.GetMask("Default");

        cam = GetComponent<MainCamera>();
        wr = GetComponent<WallRunning>();
        rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true;
        
        maxYSpeed = -1;

        startYScale = transform.localScale.y;

        readyToJump = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        MyInput();
        LimitVelocity();
        HandleDrag();
        StateHandler();

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if (grounded && doubleJumpsLeft != doubleJumps)
            ResetDoubleJumps();
        if (Input.GetKeyDown(KeyCode.J))
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 50f, whatIsGround))
            {
                JumpToPosition(hit.point, 10f);
                print("trying to jump to " + hit.point);
            }
        }
        DebugText();
    }
    
    private void FixedUpdate()
    {
        if (mm == MovementMode.walking || mm == MovementMode.sprinting || mm == MovementMode.crouching || mm == MovementMode.air)
            MovePlayer();

        else
            LimitVelocity();
    }

    #region Input, Movement & Velocity Limiting
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        if(Input.GetKey(jumpKey) && grounded && readyToJump)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        else if(Input.GetKeyDown(jumpKey) && (mm == MovementMode.air || mm == MovementMode.walljumping))
        {
            DoubleJump();
        }
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
            StartCrouch();
        if (Input.GetKeyUp(crouchKey) && crouching)
            StopCrouch();

        walking = Input.GetKey(walkKey);
    }

    private void MovePlayer()
    {
        if (restricted || tierTwoRestricted) return;
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        if (OnSlope())
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * (moveForce * 7.5f), ForceMode.Force);
        else if(grounded)
            rb.AddForce(moveDirection.normalized * (moveForce * 10f), ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * (moveForce * 10f * airMultiplier), ForceMode.Force);
    }

    private void LimitVelocity()
    {
        Vector3 rbFlatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float currYVel = rb.velocity.y;
        if (rbFlatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedFlatVelocity = rbFlatVelocity.normalized * maxSpeed;

            rb.velocity = new Vector3(limitedFlatVelocity.x, rb.velocity.y, limitedFlatVelocity.z);
        }
        
        if(maxYSpeed != -1 && currYVel > maxYSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        }
    }

    private void HandleDrag()
    {
        if (mm == MovementMode.walking || mm == MovementMode.sprinting)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    #endregion

    #region Jump Abilities

    public void Jump()
    {
        if (dashing) return;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(orientation.up * jumpForce, ForceMode.Impulse);
    }

    public void DoubleJump()
    {
        if (doubleJumpsLeft <= 0) return;
        if (mm == MovementMode.wallrunning || mm == MovementMode.climbing) return;
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float flatVelMag = flatVel.magnitude;
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.velocity = inputDirection.normalized * flatVelMag;
        rb.AddForce(orientation.up * jumpForce, ForceMode.Impulse);
        doubleJumpsLeft--;
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    public void ResetDoubleJumps()
    {
        doubleJumpsLeft = doubleJumps;
    }

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight, Vector3 startPosition = new Vector3(), float maxRestrictedTime = 1f)
    {
        tierTwoRestricted = true;
        if (startPosition == Vector3.zero) startPosition = transform.position;
        Vector3 velocity = PhysicsExtension.CalculateJumpVelocity(startPosition, targetPosition, trajectoryHeight);

        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        EnableLimitedState(flatVel.magnitude);

        velocityToSet = velocity;
        Invoke(nameof(SetVelocity), 0.05f);
        Invoke(nameof(EnableMovementNextTouchDelayed), 0.01f);

        Invoke(nameof(ResetRestrictions), maxRestrictedTime);
    }
    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        rb.velocity = velocityToSet;
        cam.DoFov(100f);
    }
    private void EnableMovementNextTouchDelayed()
    {
        enableMovementOnNextTouch = true;
    }

    public void ResetRestrictions()
    {
        if (tierTwoRestricted)
        {
            tierTwoRestricted = false;
            cam.ResetFov();
        }
        DisableLimitedState();
    }

    #endregion

    #region Crouching
    private void StartCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        crouching = true;
    }

    private void StopCrouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        crouching = false;
    }

    #endregion
    
    #region StateMachine
    MovementMode lastMovementMode;
    private void StateHandler()
    {
        bool gradualVelBoost = false;
        bool instantVelChange = false; 

        if (freeze)
        {
            mm = MovementMode.freeze;
            desiredMaxSpeed = 0f;
            instantVelChange = true;
            rb.velocity = Vector3.zero;
        }
        else if (unlimitedSpeed)
        {
            mm = MovementMode.unlimited;
            desiredMaxSpeed = 1234.5678f;
        }
        else if (limitedSpeed)
        {
            mm = MovementMode.limited;
            desiredMaxSpeed = limitedMaxSpeed;
        }
        else if (dashing)
        {
            mm = MovementMode.dashing;
            instantVelChange = true;
            speedChangeFactor = dashSpeedChangeFactor;
            desiredMaxSpeed = dashMaxSpeed;
        }
        else if (walljumping)
        {
            mm = MovementMode.walljumping;
            instantVelChange = true;
            speedChangeFactor = wallJumpSpeedChangeFactor;
            desiredMaxSpeed = wallJumpMaxSpeed;
        }
        else if (wallrunning)
        {
            mm = MovementMode.wallrunning;
            desiredMaxSpeed = sprintMaxSpeed;
        }
        else if (climbing)
        {
            mm = MovementMode.climbing;
            desiredMaxSpeed = climbMaxSpeed;
        }
        else if (sliding)
        {
            mm = MovementMode.sliding;
            if (OnSlope() && rb.velocity.y < 0.2f)
            {
                desiredMaxSpeed = slopeSlideMaxSpeed;
                gradualVelBoost = true;
            }
            else
                desiredMaxSpeed = sprintMaxSpeed;
        }
        else if (crouching && grounded)
        {
            mm = MovementMode.crouching;
            desiredMaxSpeed = crouchMaxSpeed;
        }
        else if (grounded && walking)
        {
            mm = MovementMode.walking;
            desiredMaxSpeed = walkMaxSpeed;
        }
        else if (grounded)
        {
            mm = MovementMode.sprinting;
            desiredMaxSpeed = sprintMaxSpeed;
        }
        else if (swinging)
        {
            mm = MovementMode.swinging;
            desiredMaxSpeed = swingMaxSpeed;
        }
        else
        {
            mm = MovementMode.air;

            if (desiredMaxSpeed < walkMaxSpeed)
                desiredMaxSpeed = sprintMaxSpeed;

            else
                desiredMaxSpeed = walkMaxSpeed;
        }

        bool desiredMaxSpeedHasChanged = desiredMaxSpeed != lastDesiredMaxSpeed;
        bool boostedModes = lastMovementMode == MovementMode.sliding || lastMovementMode == MovementMode.walljumping || lastMovementMode == MovementMode.dashing;
        if (desiredMaxSpeedHasChanged)
        {
            if ((gradualVelBoost && desiredMaxSpeed > maxSpeed) || boostedModes && !instantVelChange)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMaxSpeed());
            }
            else
            {
                StopAllCoroutines();
                maxSpeed = desiredMaxSpeed;
            }
        }
        lastDesiredMaxSpeed = desiredMaxSpeed;
        lastMovementMode = lastMovementMode == mm ? lastMovementMode : mm;
        cam.hbEnabled = mm == MovementMode.walking || mm == MovementMode.sprinting ? true : false;
    }

    float speedChangeFactor;
    private IEnumerator SmoothlyLerpMaxSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMaxSpeed - maxSpeed);
        float startValue = maxSpeed;
        float boostFactor = 1f;
        boostFactor = speedChangeFactor;

        while (time < difference)
        {
            maxSpeed = Mathf.Lerp(startValue, desiredMaxSpeed, time / difference);
            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f) * 2f;
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease * boostFactor;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier * boostFactor;

            yield return null;
        }

        maxSpeed = desiredMaxSpeed;
        speedChangeFactor = 1;
    }

    public void EnableLimitedState(float speedLimit)
    {
        limitedMaxSpeed = speedLimit;
        limitedSpeed = true;
    }
    public void DisableLimitedState()
    {
        limitedSpeed = false;
    }

    #endregion

    #region Variables

    public bool OnSlope()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f)) return false;
        float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
        return angle < maxSlopeAngle && angle != 0;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
    #endregion

    #region Moving Platforms
    private Rigidbody movingPlatform;
    public void AssignPlatform(Rigidbody platform)
    {
        movingPlatform = platform;
    }
    public void UnassignPlatform()
    {
        movingPlatform = null;
    }

    #endregion

    #region Collision Detection
    private bool enableMovementOnNextTouch;
    private void OnCollisionEnter(Collision collision)
    {
        bool touch = false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.collider.gameObject.layer == 9 || collision.collider.gameObject.layer == 10)
                touch = true;
        }
        GetComponent<Grappling>().OnObjectTouch();
        if (enableMovementOnNextTouch && touch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();
        }
    }
    #endregion

    #region Text Displaying

    private void DebugText()
    {
        if (text_speed != null)
        {
            Vector3 rbFlatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            text_speed.SetText("Speed: " + Round(rbFlatVelocity.magnitude, 1) + "/" + Round(maxSpeed,0));
        }

        if (text_ySpeed != null)
            text_ySpeed.SetText("Y Speed: " + Round(rb.velocity.y, 1));

        if (text_moveState != null)
            text_moveState.SetText(mm.ToString());
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    #endregion
}