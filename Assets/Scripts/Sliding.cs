using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")] 
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObject;
    private Rigidbody _rb;
    private PlayerMovement _pm;

    [Header("Sliding")] 
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideForce;
    private float _slideTimer;

    [SerializeField] private float slideYScale;
    private float _startYScale;

    [Header("Input")] 
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;
    private float _horizontalInput;
    private float _verticalInput;

    private void StartSlide()
    {
        _pm.isSliding = true;

        playerObject.localScale = new Vector3(playerObject.localScale.x, slideYScale, playerObject.localScale.z);
        _rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        _slideTimer = maxSlideTime;
    }
    
    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;

        if (!_pm.OnSlope() || _rb.velocity.y > -0.1f)
        {
            _rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            _slideTimer -= Time.deltaTime;
        }
        else
        {
            _rb.AddForce(_pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (_slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        _pm.isSliding = false;
        
        playerObject.localScale = new Vector3(playerObject.localScale.x, _startYScale, playerObject.localScale.z);
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _pm = GetComponent<PlayerMovement>();

        _startYScale = playerObject.localScale.y;
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        
        if (Input.GetKeyDown(slideKey) && (_horizontalInput != 0 || _verticalInput != 0))
            StartSlide();
        
        if (Input.GetKeyUp(slideKey) && _pm.isSliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (_pm.isSliding)
            SlidingMovement();
    }
}
