using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public bool isSliding;
    [HideInInspector] public bool isWallRunning;
    [HideInInspector] public bool isClimbing;
    [HideInInspector] public bool isFreezing;
    [HideInInspector] public bool isUnlimited;
    [HideInInspector] public bool isLimited;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isRestricted;
    [HideInInspector] public bool isTierTwoRestricted;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public bool isActiveGrapple;
    [HideInInspector] public bool isSwinging;

    [SerializeField] private MovementState state;
    [SerializeField] private Transform orientation;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private float climbSpeed;
    [SerializeField] private float airMinSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashSpeedChangeFactor;
    [SerializeField] private float swingMaxSpeed;
    [SerializeField] private float limitedMaxSpeed;

    public float maxYSpeed;
    public float _moveSpeed;
    private float _desireMoveSpeed;
    private float _lastDesiredMoveSpeed;
    private bool isKeepingMomentum;
    private MovementState lastState;

    [Header("Crouching")] 
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYscale;
    private float _startYScale;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool _readyToJump;
    
    [Header("Ground Check")] 
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Keybinds")] 
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    //[SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Slope Handling")] 
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    [Header("Camera Effects")] 
    [SerializeField] private MainCamera cam;
    [SerializeField] private float grappleFOV = 95f;

    [Header("References")] 
    [SerializeField] private Climbing climbing;

    private float _horizontalInput;
    private float _verticalInput;
    
    private Vector3 _movementDirection;
    private Rigidbody _rb;

    private enum MovementState
    {
        Freeze,
        Unlimited,
        Limited,
        Walking,
        Sprinting,
        Crouching,
        Dashing,
        Climbing,
        Sliding,
        WallRunning,
        Swinging,
        Air
    }

    private void StateHandler()
    {
        isKeepingMomentum = false;
        bool hasInstantChange = false;
        if (isDashing)
        {
            state = MovementState.Dashing;
            hasInstantChange = true;
            _desireMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else if (isFreezing)
        {
            state = MovementState.Freeze;
            _rb.velocity = Vector3.zero;
            hasInstantChange = true;
            _desireMoveSpeed = 0f;
        }
        else if (isUnlimited)
        {
            state = MovementState.Unlimited;
            _desireMoveSpeed = 100f;
            return;
        } else if (isLimited)
        {
            state = MovementState.Limited;
            _desireMoveSpeed = limitedMaxSpeed;
        }
        else if (isClimbing)
        {
            state = MovementState.Climbing;
            _desireMoveSpeed = climbSpeed;
        }
        else if (isWallRunning)
        {
            state = MovementState.WallRunning;
            _desireMoveSpeed = wallRunSpeed;
            isKeepingMomentum = false;
        }
        else if (isSliding)
        {
            state = MovementState.Sliding;

            if (OnSlope() && _rb.velocity.y < 0.1f)
            {
                _desireMoveSpeed = slideSpeed;
                isKeepingMomentum = true;
            } else 
                _desireMoveSpeed = sprintSpeed;
        }
        else if (Input.GetKey(crouchKey) && isGrounded)
        {
            state = MovementState.Crouching;
            _desireMoveSpeed = crouchSpeed;
        }
        else if (isGrounded)
        {
            state = MovementState.Walking;
            _desireMoveSpeed = walkSpeed;
        }
        else if (isSwinging)
        {
            state = MovementState.Swinging;
            _desireMoveSpeed = swingMaxSpeed;
        }
        else
        {
            state = MovementState.Air;

            if (_desireMoveSpeed < walkSpeed)
                _desireMoveSpeed = sprintSpeed;
            else
                _desireMoveSpeed = walkSpeed;
            
            if (_moveSpeed < airMinSpeed)
                _desireMoveSpeed = airMinSpeed;
        }

        bool hasDesiredMoveSpeedChanged = _desireMoveSpeed != _lastDesiredMoveSpeed;
        bool boostedModes = lastState == MovementState.Dashing || lastState == MovementState.Sliding;

        if (hasDesiredMoveSpeedChanged)
        {
            if (isKeepingMomentum && _desireMoveSpeed > _moveSpeed || boostedModes && !hasInstantChange)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                _moveSpeed = _desireMoveSpeed;
            }
        }
        
        _lastDesiredMoveSpeed = _desireMoveSpeed;
        lastState = state;

        if (Mathf.Abs(_desireMoveSpeed - _moveSpeed) < 0.1f) isKeepingMomentum = false;
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(_desireMoveSpeed - _moveSpeed);
        float startValue = _moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            _moveSpeed = Mathf.Lerp(startValue, _desireMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f) * 2f;
                
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease * boostFactor;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier * boostFactor;
            }
            
            yield return null;
        }

        _moveSpeed = _desireMoveSpeed;
        speedChangeFactor = 1f;
        isKeepingMomentum = false;
    }
    
    private void MyInput()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && _readyToJump && isGrounded)
        {
            _readyToJump = false;
            
            Jump();
            
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey) && _horizontalInput == 0 && _verticalInput == 0)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYscale, transform.localScale.z);
            _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
        }
    }

    private void MovePlayer()
    {
        if (isActiveGrapple) return;
        if (isSwinging) return;
        if (state == MovementState.Dashing) return;
        if (climbing.isExitingWall) return;
        if (isRestricted || isTierTwoRestricted) return;
        //if (climbing.isExitingWall) return;
        
        _movementDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

        if (OnSlope() && !_exitingSlope)
        {
            _rb.AddForce(GetSlopeMoveDirection(_movementDirection) * (_moveSpeed * 20f), ForceMode.Force);

            if (_rb.velocity.y > 0)
            {
                _rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else switch (isGrounded)
        {
            case true:
                _rb.AddForce(_movementDirection.normalized * (_moveSpeed * 10f), ForceMode.Force);
                break;
            case false:
                _rb.AddForce(_movementDirection.normalized * (_moveSpeed * 10f * airMultiplier), ForceMode.Force);
                break;
        }

        if (!isWallRunning) _rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (isActiveGrapple) return;
        
        if (OnSlope() && !_exitingSlope)
        {
            if (_rb.velocity.magnitude > _moveSpeed)
            {
                _rb.velocity = _rb.velocity.normalized * _moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

            if (!(flatVel.magnitude > _moveSpeed)) return;
            Vector3 limitedVel = flatVel.normalized * _moveSpeed;
            _rb.velocity = new Vector3(limitedVel.x, _rb.velocity.y, limitedVel.z);
        }

        if (maxYSpeed != -1 && _rb.velocity.y > maxYSpeed)
            _rb.velocity = new Vector3(_rb.velocity.x, maxYSpeed, _rb.velocity.z);
    }

    private void Jump()
    {
        if (isDashing) return;
        _exitingSlope = true;
        
        _rb.velocity = new Vector3(_rb.velocity.x, 0.0f, _rb.velocity.z);
        
        _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        print("Jumping");
    }

    private void ResetJump()
    {
        _readyToJump = true;

        _exitingSlope = false;
    }

    private bool enableMovementOnNexttouch;

    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight, Vector3 startPosition = new Vector3(), float maxRestrictedTime = 1f)
    {
        isTierTwoRestricted = true;
        isActiveGrapple = true;

        if (startPosition == Vector3.zero) startPosition = transform.position;
        
        Vector3 velocity = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);

        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        EnableLimitedState(flatVel.magnitude);

        velocityToSet = velocity;
        
        Invoke(nameof(SetVelocity), 0.05f);
        Invoke(nameof(EnableMovementNextTouchDelayed), 0.01f);
        
        Invoke(nameof(ResetRestriction), maxRestrictedTime);
    }

    private Vector3 velocityToSet;
    
    private void SetVelocity()
    {
        _rb.velocity = velocityToSet;
        
        cam.DoFov(grappleFOV);
    }

    private void EnableMovementNextTouchDelayed()
    {
        enableMovementOnNexttouch = true;
    }

    private void EnableLimitedState(float speedLimit)
    {
        limitedMaxSpeed = speedLimit;
        isLimited = true;
    }

    public void DisableLimitedState()
    {
        isLimited = false;
    }

    public void ResetRestriction()
    {
        if (isTierTwoRestricted)
        {
            isActiveGrapple = false;
            isTierTwoRestricted = false;
            cam.DoFov(85f);
        }
        
        DisableLimitedState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNexttouch)
        {
            enableMovementOnNexttouch = false;
            ResetRestriction();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, _slopeHit.normal).normalized;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
                             + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
        _readyToJump = true;

        maxYSpeed = -1;

        _startYScale = transform.localScale.y;
    }
    
    private void Update()
    {
        MyInput();
        SpeedControl();
        StateHandler();
        if (isRestricted)
        {
            print("I am restricted");
        }

        if (isTierTwoRestricted)
        {
            print("I am tier two restricted");
        }
        
        if (state == MovementState.Dashing)
            print("Dashing");
        
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if ((state == MovementState.Walking || state == MovementState.Sprinting || state == MovementState.Crouching) && !isActiveGrapple)
            _rb.drag = groundDrag;
        else
            _rb.drag = 0;
    }

    private void FixedUpdate()
    {
        if (state == MovementState.Walking || state == MovementState.Crouching || state == MovementState.Air)
            MovePlayer();
        else
            SpeedControl();
    }
}
