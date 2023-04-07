using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AInteractPlay : MonoBehaviour
{
    private Camera _cam;
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;
    private PlayerUI _playerUI;
    void Start()
    {
        _cam = GetComponent<MainCamera>().cam;
        _playerUI = GetComponent<PlayerUI>();
    }

    void Update()
    {
        _playerUI.UpdateText(string.Empty);
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            if (hit.collider.GetComponent<AInteractable>() != null)
            {
                AInteractable interactable = hit.collider.GetComponent<AInteractable>();
                _playerUI.UpdateText(interactable.promptMessage);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.BaseInteract();
                }
            }
        }
    }
}
