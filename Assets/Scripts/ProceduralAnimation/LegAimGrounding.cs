using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegAimGrounding : MonoBehaviour
{
    private GameObject raycastOrigin;
    private int layerMask;
    void Start()
    {
        layerMask = LayerMask.GetMask("whatIsGround");
        raycastOrigin = transform.gameObject;
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(raycastOrigin.transform.position, -transform.up, out hit, Mathf.Infinity, layerMask))
        {
            transform.position = hit.point;
        }
    }
}
