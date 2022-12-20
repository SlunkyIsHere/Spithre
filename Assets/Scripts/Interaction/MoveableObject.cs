using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour, IInteractable
{
    public bool stayStill;
    public bool useHitPoint;

    public Rigidbody rb;

    private Vector3 stickPoint;
    private float force;

    private PlayerInteractions interactions;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        interactions = FindObjectOfType<PlayerInteractions>();
        interactions.RemoveSelectedMaterials += ResetMaterial;
    }

    void Update()
    {
        Vector3 dirToStickPoint = stickPoint - transform.position;

        rb.AddForce(dirToStickPoint.normalized * force * Time.deltaTime);
    }

    public void AddStickPoint(Vector3 point, float _force)
    {
        stickPoint = point;
        force = _force;
    }

    public void SetMaterial(Material material)
    {
        GetComponent<MeshRenderer>().material = material;
    }

    public void ResetMaterial()
    {
        GetComponent<MeshRenderer>().material = interactions.mat_notSelected;
    }
}
