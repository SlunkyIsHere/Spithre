using UnityEngine;

/*
 * This script is mostly inspired by BuffaLou,
 * who made this amazing video, explaining the theory of
 * headbobbing and swaying:
 * https://www.youtube.com/watch?v=DR4fTllQnXg&list=PL9CfZshMYXhZYaWpHxkAiJlgKj5huQRmw&index=3
 */

namespace Weapons
{
    public class WeaponSway : MonoBehaviour
    { 
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private Rigidbody rb;
    
        private Vector2 _walkInput;
        private Vector2 _lookInput;
    
        [Header("Sway")]
        [SerializeField] private float step = 0.01f;
        [SerializeField] private float maxStepDistance = 0.06f;
        private Vector3 _swayPos;

        [Header("Sway Rotation")]
        [SerializeField] private float rotationStep = 4f;
        [SerializeField] private float maxRotationStep = 5f;
        private Vector3 _swayEulerRot; 

        [SerializeField] private float smooth = 10f;
        private const float SmoothRot = 12f;

        [Header("Bobbing")]
        [SerializeField] private float speedCurve;

        private float CurveSin => Mathf.Sin(speedCurve);
        private float CurveCos => Mathf.Cos(speedCurve);

        [SerializeField] private Vector3 travelLimit = Vector3.one * 0.025f;
        [SerializeField] private Vector3 bobLimit = Vector3.one * 0.01f;
        private Vector3 _bobPosition;
    
        [Header("Bob Rotation")]
        [SerializeField] private Vector3 multiplier;
        private Vector3 _bobEulerRotation;

        [Header("Jumping Sway")] 
        [SerializeField] private float jumpMultiplier = 2f;

        void Update()
        {
            GetInput();

            Sway();
            SwayRotation();
            BobOffset();
            BobRotation();

            CompositePositionRotation();
        }

        void GetInput(){
            _walkInput.x = Input.GetAxisRaw("Horizontal");
            _walkInput.y = Input.GetAxisRaw("Vertical");
            _walkInput = _walkInput.normalized;

            _lookInput.x = Input.GetAxis("Mouse X");
            _lookInput.y = Input.GetAxis("Mouse Y");
        }


        void Sway(){
            Vector3 invertLook = _lookInput *-step;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

            _swayPos = invertLook;
        }

        void SwayRotation(){
            Vector2 invertLook = _lookInput * -rotationStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
            _swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }

        void CompositePositionRotation(){
            transform.localPosition = Vector3.Lerp(transform.localPosition, _swayPos + _bobPosition, Time.deltaTime * smooth);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(_swayEulerRot) * Quaternion.Euler(_bobEulerRotation), Time.deltaTime * SmoothRot);
        }

        void BobOffset(){
            speedCurve += Time.deltaTime * (playerMovement.grounded ? rb.velocity.magnitude : 1f) + 0.01f;

            _bobPosition.x = CurveCos * bobLimit.x * (playerMovement.grounded ? 1 : 0) - _walkInput.x * travelLimit.x;
            if (playerMovement.grounded)
            {
                _bobPosition.y = CurveSin * bobLimit.y - Input.GetAxis("Vertical") * travelLimit.y;
            }
            else
            {
                _bobPosition.y = jumpMultiplier * (CurveSin * bobLimit.y - Input.GetAxis("Vertical") * travelLimit.y);
            }
            _bobPosition.z = -(_walkInput.y * travelLimit.z);
        }

        void BobRotation(){
            _bobEulerRotation.x = _walkInput != Vector2.zero ? multiplier.x * Mathf.Sin(2 * speedCurve) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 2);
            _bobEulerRotation.y = _walkInput != Vector2.zero ? multiplier.y * CurveCos : 0;
            _bobEulerRotation.z = _walkInput != Vector2.zero ? multiplier.z * CurveCos * _walkInput.x : 0;
        }
    }
}
