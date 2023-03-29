using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public Grappling grappling;
    public PlayerInteractions playerInteractions;
    [HideInInspector] public enum AbilityType
    {
        GrappleHook,
        Swinging,
        ObjectMoving
    }
 
    // Variable to store the chosen ability
    private AbilityType currentAbility;
 
    void Start()
    {
        // Set the default ability
        SetAbility(AbilityType.GrappleHook);
    }
 
    void Update()
    {
        // Check for right mouse button press
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            PlayAbility();
        }

        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            grappling.StopSwing();
            grappling.TryStopGrapple();
        }
    }
 
    // SetAbility function (call this function from the pieWheel buttons and set the corresponding ability)
    public void SetAbility(AbilityType newAbility)
    {
        currentAbility = newAbility;
    }
 
    // PlayAbility function
    public void PlayAbility()
    {
        switch (currentAbility)
        {
            case AbilityType.GrappleHook:
                grappling.StartGrapple();
                break;
            case AbilityType.Swinging:
                grappling.StartSwing();
                break;
            case AbilityType.ObjectMoving:
            {
                if (playerInteractions.CheckForInteractions()) playerInteractions.Interact();
                break;
            }
        }
    }
}
