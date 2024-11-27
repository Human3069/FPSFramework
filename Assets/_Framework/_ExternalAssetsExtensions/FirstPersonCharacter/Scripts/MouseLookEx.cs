using System;
using UnityEngine;

// This class is created for the example scene. There is no support for this script.
namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLookEx
    {
        private const string LOG_FORMAT = "<color=white><b>[MouseLookEx]</b></color> {0}";

        public float XSensitivity = 1f;
        public float YSensitivity = 1f;

        [Space(10)]
        [SerializeField]
        private bool clampVerticalRotation = true;

        [Space(10)]
        [SerializeField]
        private float minimumX = -90F;
        [SerializeField]
        private float maximumX = 90F;

        [Space(10)]
        [SerializeField]
        private bool smooth;
        [SerializeField]
        private float smoothTime = 5f;

        private Quaternion _characterTargetRot;
        private Quaternion _cameraTargetRot;

        public void Init(Transform character, Transform camera)
        {
            _characterTargetRot = character.localRotation;
            _cameraTargetRot = camera.localRotation;
        }

        public void UpdateCursorLock(bool isLock)
        {
            if (isLock == true)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        public void LookRotation(Transform characterT, Transform cameraT)
        {
            float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            _characterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            _cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

            if (clampVerticalRotation == true)
            {
                _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot);
            }

            if (smooth == true)
            {
                characterT.localRotation = Quaternion.Slerp(characterT.localRotation, _characterTargetRot, smoothTime * Time.deltaTime);
                cameraT.localRotation = Quaternion.Slerp(cameraT.localRotation, _cameraTargetRot, smoothTime * Time.deltaTime);
            }
            else
            {
                characterT.localRotation = _characterTargetRot;
                cameraT.localRotation = _cameraTargetRot;
            }
        }

        public void SetRotation(Transform characterT, Transform cameraT, Vector3 angle)
        {
            Debug.LogFormat(LOG_FORMAT, "SetRotation(), angle : " + angle);

            _characterTargetRot = Quaternion.Euler(0f, angle.y, 0f);
            _cameraTargetRot = Quaternion.Euler(angle.x, 0f, 0f);

            if (clampVerticalRotation == true)
            {
                _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot);
            }

            if (smooth == true)
            {
                characterT.localRotation = Quaternion.Slerp(characterT.localRotation, _characterTargetRot, smoothTime * Time.deltaTime);
                cameraT.localRotation = Quaternion.Slerp(cameraT.localRotation, _cameraTargetRot, smoothTime * Time.deltaTime);
            }
            else
            {
                characterT.localRotation = _characterTargetRot;
                cameraT.localRotation = _cameraTargetRot;
            }
        }

        private Quaternion ClampRotationAroundXAxis(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
            angleX = Mathf.Clamp(angleX, minimumX, maximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }
    }
}
