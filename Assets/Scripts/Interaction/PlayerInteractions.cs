using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Settings")]
    public Transform camT;
    public float interactionSpherecastRadius;
    public float interactionDistance;
    public LayerMask whatIsInteractable;
    public KeyCode interactionKey = KeyCode.Mouse1;

    [Header("Moving")]
    public float moveForce;

    [Header("Graphics")]
    public GameObject interactionSign;
    public Material mat_notSelected;
    public Material mat_selected;

    private IInteractable currInteractable;
    private MoveableObject moveableObject1;
    private MoveableObject moveableObject2;
    private Transform interactableT = null;
    private Vector3 interactionPoint = Vector3.zero;
    public bool interactableActive;
    private bool lastInteractableActive;

    private bool isFirstInteraction;


    #region Check for interactions and interact

    private void Start()
    {
        isFirstInteraction = true;
        RemoveInteractionSign();
    }

    private void Update()
    {
        //CheckForInteractions();

        if (Input.GetKeyDown(interactionKey) && CheckForInteractions())
            Interact();
    }

    private void Interact()
    {
        // first step
        if (moveableObject1 == null)
        {
            moveableObject1 = interactableT.GetComponent<MoveableObject>();
            isFirstInteraction = false;
            print("stored interaction info 1");
        }

        // second step
        else if (moveableObject2 == null)
        {
            moveableObject2 = interactableT.GetComponent<MoveableObject>();
            isFirstInteraction = true;
            print("stored interaction info 2");

            MoveObjects();
        }
    }

    public void MoveObjects()
    {
        if (!moveableObject1.stayStill && !moveableObject2.stayStill)
        {
            Vector3 dirFrom1To2 = moveableObject2.transform.position - moveableObject1.transform.position;
            moveableObject1.AddStickPoint(moveableObject1.transform.position + dirFrom1To2 * 0.5f, moveForce);

            Vector3 dirFrom2To1 = moveableObject1.transform.position - moveableObject2.transform.position;
            moveableObject2.AddStickPoint(moveableObject2.transform.position + dirFrom2To1 * 0.5f, moveForce);
        }

        if(!moveableObject1.stayStill && moveableObject2.stayStill)
        {
            moveableObject1.AddStickPoint(moveableObject2.transform.position, moveForce);
        }

        if(moveableObject1.stayStill && !moveableObject2.stayStill)
        {
            moveableObject2.AddStickPoint(moveableObject1.transform.position, moveForce);
        }

        moveableObject1 = null;
        moveableObject2 = null;

        //Vector3 dirFrom2To1 = moveableObject1.transform.position - moveableObject2.transform.position;
        //moveableObject1.AddForce(dirFrom2To1.normalized * moveForce);
    }

    private bool CheckForInteractions()
    {
        RaycastHit hit;
        if (Physics.SphereCast(camT.position, interactionSpherecastRadius, camT.forward, out hit, interactionDistance, whatIsInteractable))
        {
            // store interactable for later
            if (hit.transform.gameObject.TryGetComponent(out IInteractable interactable))
            {
                currInteractable = interactable;
                interactableT = hit.transform;
                interactionPoint = hit.transform.position;

                PlaceInteractionSign();
                return true;
            }
            else
            {
                RemoveInteractionSign();
                return false;
            }
        }
        else
        {
            RemoveInteractionSign();
            return false;
        }
    }

    #endregion


    #region Place and remove interaction sign

    public void PlaceInteractionSign()
    {
        if (currInteractable != null) currInteractable.SetMaterial(mat_selected);
        interactionSign.SetActive(true);
    }

    public Action RemoveSelectedMaterials;

    public void RemoveInteractionSign()
    {
        if (RemoveSelectedMaterials != null) RemoveSelectedMaterials();
        interactionSign.SetActive(false);
        currInteractable = null;
    }

    #endregion
}
