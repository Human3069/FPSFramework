using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

// This class is created for the example scene. There is no support for this script.
namespace UnityStandardAssets.Utility
{
    [Serializable]
    public class FOVKickEx
    {
        [SerializeField]
        private CinemachineVirtualCamera _vCam;

        [SerializeField]
        private float fovIncrease = 3f;
        [SerializeField]
        private float timeToIncrease = 1f;
        [SerializeField]
        private float timeToDecrease = 1f;

        [Space(10)]
        [SerializeField]
        private AnimationCurve increaseCurve;

        private float _originalFov;

        public void Setup(CinemachineVirtualCamera vCam)
        {
            this._vCam = vCam;
            this._originalFov = vCam.m_Lens.FieldOfView;

            CheckStatus();
        }


        private void CheckStatus()
        {
            if (_vCam == null)
            {
                throw new Exception("FOVKick camera is null, please supply the camera to the constructor");
            }

            if (increaseCurve == null)
            {
                throw new Exception("FOVKick Increase curve is null, please define the curve for the field of view kicks");
            }
        }

        public IEnumerator FOVKickUp()
        {
            float _time = Mathf.Abs((_vCam.m_Lens.FieldOfView - _originalFov) / fovIncrease);
            while (_time < timeToIncrease)
            {
                _vCam.m_Lens.FieldOfView = _originalFov + (increaseCurve.Evaluate(_time / timeToIncrease) * fovIncrease);
                _time += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator FOVKickDown()
        {
            float _time = Mathf.Abs((_vCam.m_Lens.FieldOfView - _originalFov) / fovIncrease);
            while (_time > 0)
            {
                _vCam.m_Lens.FieldOfView = _originalFov + (increaseCurve.Evaluate(_time / timeToDecrease) * fovIncrease);
                _time -= Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            _vCam.m_Lens.FieldOfView = _originalFov;
        }
    }
}
