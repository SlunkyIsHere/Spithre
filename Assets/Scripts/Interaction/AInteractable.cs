using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AInteractable : MonoBehaviour
{
    public bool useEvents;
    
    public string promptMessage;

    public void BaseInteract()
    {
        if (useEvents)
            GetComponent<InteractionEvent>().OnInteract.Invoke();
        
        Interact();
    }

    protected virtual void Interact()
    {
        
    }
}
