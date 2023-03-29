using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieMenuBehaviour : MonoBehaviour
{
    [SerializeField] private float timeScale;
    [SerializeField] private bool isActive;
    [SerializeField] private KeyCode pieKey = KeyCode.Q;
    [SerializeField] private GameObject pieMenu;
    [SerializeField] private ProjectileGun projectileGun;
    [SerializeField] private RaycastGun raycastGun;
    
    private float _originalTimeScale;
    void Start()
    {
        _originalTimeScale = Time.timeScale;
    }

    void Update()
    {

        if (Input.GetKey(pieKey))
        {
            Time.timeScale = timeScale;
            
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            //projectileGun.readyToShoot = false;
            //raycastGun.isReadyToShoot = false;
            pieMenu.SetActive(true);
        }

        if (Input.GetKeyUp(pieKey))
        {
            Time.timeScale = _originalTimeScale;
            
            Cursor.lockState = CursorLockMode.Locked;

            //projectileGun.readyToShoot = true;
            //raycastGun.isReadyToShoot = true;
            
            pieMenu.SetActive(false);
        }
    }

}
 