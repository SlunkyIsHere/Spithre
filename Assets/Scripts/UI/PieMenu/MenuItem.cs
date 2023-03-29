using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color baseColor;
    [SerializeField] private Image background;

    public void OnUse()
    {
        Debug.Log("I'm used");
    }
    
    void Start()
    {
        background.color = baseColor;
    }

    public void Select()
    {
        background.color = hoverColor;
    }

    public void Deselect()
    {
        background.color = baseColor;
    }
}
