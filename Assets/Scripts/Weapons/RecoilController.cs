using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * This script was heavily inspired by tutorial from Gilbert:
 * https://www.youtube.com/watch?v=geieixA4Mqc
 */

namespace Weapons
{
    public class RecoilController : MonoBehaviour
    {
        public Transform playerCam;
        public float recoilSpeed = 1.0f;
        public float returnSpeed = 2.0f;
        public Vector2 recoilAmount = new Vector2(1.5f, 3.0f);
        public Vector2 rotationRange = new Vector2(-5.0f, 5.0f);

        private Vector3 originalRotation;
        private Vector3 currentRecoil;

        void Awake()
        {
            originalRotation = playerCam.localRotation.eulerAngles;
        }

        public void ApplyRecoil()
        {
            currentRecoil += new Vector3(-Random.Range(recoilAmount.x, recoilAmount.y), Random.Range(rotationRange.x, rotationRange.y), 0.0f);
        }

        void Update()
        {
            Debug.Log(playerCam.localRotation);
            playerCam.localRotation = Quaternion.Slerp(playerCam.localRotation,
                Quaternion.Euler(originalRotation + currentRecoil), Time.deltaTime * recoilSpeed);
            currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, Time.deltaTime * returnSpeed);
        }
    }
}
