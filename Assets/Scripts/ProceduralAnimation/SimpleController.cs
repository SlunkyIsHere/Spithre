using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleController : MonoBehaviour
{
    public Rigidbody controller;

    public float speed = 6;
    private Vector3 velocity;
    private float trunSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    void Update()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f && controller.velocity.magnitude < 4)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.AddForce(moveDir.normalized * (speed * Time.deltaTime));
        }
    }
}
