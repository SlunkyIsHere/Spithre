using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieMenu : MonoBehaviour
{
    [SerializeField] private Vector2 normalizedMousePosition;
    [SerializeField] private float currentAngle;
    [SerializeField] private int selection;
    [SerializeField] private int amountOfItems;
    private int _previousSelection;
    public AbilityManager _abilityManager;

    [SerializeField] private GameObject[] menuItems;

    private MenuItem _menuItem;
    private MenuItem _previousMenuItem;

    void Update()
    {
        normalizedMousePosition = new Vector2(Input.mousePosition.x - Screen.width / 2,
            Input.mousePosition.y - Screen.height / 2);
        currentAngle = Mathf.Atan2(normalizedMousePosition.y, normalizedMousePosition.x) * Mathf.Rad2Deg;

        currentAngle = (currentAngle + 360) % 360;

        selection = (int)currentAngle / (360 / menuItems.Length);

        if (selection != _previousSelection)
        {
            _previousMenuItem = menuItems[_previousSelection].GetComponent<MenuItem>();
            _previousMenuItem.Deselect();
            _previousSelection = selection;

            _menuItem = menuItems[selection].GetComponent<MenuItem>();
            _menuItem.Select();
        }

        //if (Input.GetMouseButtonDown(0))
        //{
            switch (selection)
            {
                case 0:
                    _abilityManager.SetAbility(AbilityManager.AbilityType.Swinging);
                    break;
                case 1:
                    _abilityManager.SetAbility(AbilityManager.AbilityType.ObjectMoving);
                    break;
                case 2:
                    _abilityManager.SetAbility(AbilityManager.AbilityType.GrappleHook);
                    break;
            }
        //}
        
        Debug.Log(selection);
    }
}
