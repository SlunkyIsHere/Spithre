using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")] 
    private PlayerMovement pm;
    [SerializeField] private Transform cam;
    [SerializeField] public Transform gunTip;
    [SerializeField] private LayerMask whatIsGrappleable;
    [SerializeField] private Transform orientation;
    
    [Header("Swinging")] 
    [SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] private float spring = 4.5f;
    [SerializeField] private float damper = 7f;
    [SerializeField] private float massScale = 4.5f;
    [SerializeField] private bool isEnabledSwingWithForce = true;
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
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse0;

    [Header("WebMove")]
    [SerializeField] private float horizontalThrustForce;
    [SerializeField] private float forwardThrustForce;
    [SerializeField] private float extendCableSpeed;
    
    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;

    private bool isGrappling;
    private bool isTracking;
    private bool isGrappleExecuted;
    private Rigidbody rb;

    private void StartGrapple()
    {
        if (grapplingCoolDownTimer > 0) return;
        
        StopSwing();

        grapplingCoolDownTimer = grapplingCoolDown;
        
        isGrappling = true;
        
        pm.isFreezing = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grappleObject = hit.transform;
            isTracking = true;
            
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;
            
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }
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
        isGrappleExecuted = true;
    }

    public void TryStopGrapple()
    {
        if (!isGrappleExecuted) return;
        
        StopGrapple();
    }

    public void StopGrapple()
    {
        if (pm.isFreezing) pm.isFreezing = false;
        
        isGrappling = false;
        isGrappleExecuted = false;
    }

    public void OnObjectTouch()
    {
        if (isGrappleExecuted) StopGrapple();
    }

    private void CheckForSwingPoints()
    {
        if (joint != null) return;

        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, out var sphereCastHit, 
            maxSwingDistance, whatIsGrappleable);

        Physics.Raycast(cam.position, cam.forward, out var raycastHit, 
            maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;

        if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;
        else 
            realHitPoint = Vector3.zero;

        if (realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        } else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }
    
    private Transform grappleObject;
    private void StartSwing()
    {
        if (predictionHit.point == Vector3.zero) return;
        
        pm.isSwinging = true;
        
        grapplePoint = predictionHit.point;
        joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        grappleObject = predictionHit.transform;
        isTracking = true;

        float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = spring;
        joint.damper = damper;
        joint.massScale = massScale;
        
    }

    public void StopSwing()
    {
        pm.isSwinging = false;

        isTracking = false;

        Destroy(joint);
    }

    private void WebMovement()
    {
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * (horizontalThrustForce * Time.deltaTime));
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * (horizontalThrustForce * Time.deltaTime));
        if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * (forwardThrustForce * Time.deltaTime));
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = grapplePoint - transform.position;
            rb.AddForce(directionToPoint.normalized * (forwardThrustForce * Time.deltaTime));

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint) + extendCableSpeed;
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }
    }

    public bool IsGrappling()
    {
        return joint != null || isGrappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 direction = (grapplePoint - transform.position).normalized;
        Gizmos.DrawRay(transform.position, direction * maxGrappleDistance);
    }

    private void MyInput()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();
        if (Input.GetKeyUp(grappleKey)) TryStopGrapple();
        
        if (Input.GetKeyDown(swingKey)) StartSwing();
        if (Input.GetKeyUp(swingKey)) StopSwing();
    }
    
    private void Start()
    {
        if (whatIsGrappleable == 0)
            whatIsGrappleable = LayerMask.GetMask("Default");
        
        pm = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (grapplingCoolDownTimer > 0)
            grapplingCoolDownTimer -= Time.deltaTime;
        
        MyInput();
        
        CheckForSwingPoints();

        if (isEnabledSwingWithForce && joint != null) WebMovement();
    }
}
