﻿// Designed by KINEMATION, 2023

using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.Attributes;
using UnityEngine.Events;

namespace Kinemation.FPSFramework.Runtime.Camera
{
    [Serializable]
    public struct CameraShakeInfo
    {
        public VectorCurve shakeCurve;
        public Vector4 pitch;
        public Vector4 yaw;
        public Vector4 roll;
        public float smoothSpeed;
        public float playRate;

        public CameraShakeInfo(Keyframe[] frames, float playRate, float smooth)
        {
            shakeCurve = new VectorCurve(frames);
            pitch = new Vector4(0f, 0f, 0f, 0f);
            yaw = new Vector4(0f, 0f, 0f, 0f);
            roll = new Vector4(0f, 0f, 0f, 0f);
            smoothSpeed = smooth;
            this.playRate = playRate;
        }
    }
    
    public class FPSCamera : MonoBehaviour
    {
        public CameraData cameraData;
        public Transform rootBone;

        [ReadOnly]
        [SerializeField]
        protected bool _isAiming = false;
        public bool IsAiming
        {
            get
            {
                return _isAiming;
            }
            set
            {
                if (_isAiming != value)
                {
                    _isAiming = value;

                    OnAimedEvent.Invoke(value);
                }
            }
        }

        public UnityEvent<bool> OnAimedEvent;

        public bool useViewLimits = false;
        public bool useCameraAnimation = true;
        
        // private UnityEngine.Camera _mainCamera;

        private CameraShakeInfo _shake;
        private Vector3 _target;
        private Vector3 _out;
        private Quaternion _animation;
        private float _playBack = 0f;

#if false
        private float _fovPlayback = 0f;

        private float _fov = 0f;
#endif

        private Vector2 _pitchLimit = new Vector2(-90f, 90f);
        private Vector2 _yawLimit = new Vector2(-90f, 90f);
        
        private Vector2 _cachePitchLimit = new Vector2(-90f, 90f);
        private Vector2 _cacheYawLimit = new Vector2(-90f, 90f);
        private float _viewLimitSpeed = 1f;
        
        private float _viewLimitPlayback = 0f;

        private float GetRandomTarget(Vector4 target)
        {
            return Random.Range(Random.Range(target.x, target.y), Random.Range(target.z, target.w));
        }
        
        private void UpdateShake()
        {
            if (useCameraAnimation)
            {
                transform.rotation *= _animation;
            }

            if (!_shake.shakeCurve.IsValid())
            {
                return;
            }

            _playBack += _shake.playRate * Time.deltaTime;
            _playBack = Mathf.Min(_playBack, _shake.shakeCurve.GetLastTime());

            Vector3 curveValue = _shake.shakeCurve.Evaluate(_playBack);

            _out.x = CoreToolkitLib.Interp(_out.x, curveValue.x * _target.x, _shake.smoothSpeed, Time.deltaTime);
            _out.y = CoreToolkitLib.Interp(_out.y, curveValue.y * _target.y, _shake.smoothSpeed, Time.deltaTime);
            _out.z = CoreToolkitLib.Interp(_out.z, curveValue.z * _target.z, _shake.smoothSpeed, Time.deltaTime);

            Quaternion rot = Quaternion.Euler(_out);
            transform.rotation *= rot;
        }

        private void UpdateFOV()
        {
#if false
            if (_mainCamera == null || cameraData == null) return;

            float alpha = cameraData.fovCurve.Evaluate(_fovPlayback);
            float hFOV = Mathf.Lerp(cameraData.baseFOV, cameraData.aimFOV, alpha);

            float vFOVrad = 2.0f * Mathf.Atan(Mathf.Tan(hFOV * Mathf.Deg2Rad / 2.0f) / _mainCamera.aspect)
                                 * Mathf.Rad2Deg;

            _fov = CoreToolkitLib.InterpLayer(_fov, vFOVrad, cameraData.extraSmoothing, Time.deltaTime);
            _mainCamera.fieldOfView = _fov;

            _fovPlayback += Time.deltaTime * cameraData.aimSpeed * (IsAiming ? 1f : -1f);
            _fovPlayback = Mathf.Clamp01(_fovPlayback);
#endif
        }

        protected void UpdateViewLimit()
        {
            if (!useViewLimits || rootBone == null) return;
            
            var pitchLimit = Vector2.Lerp(_cachePitchLimit, _pitchLimit, _viewLimitPlayback);
            var yawLimit = Vector2.Lerp(_cacheYawLimit, _yawLimit, _viewLimitPlayback);

            Vector3 rotEuler = (Quaternion.Inverse(rootBone.rotation) * transform.rotation).eulerAngles;

            if (rotEuler.x > 180f) rotEuler.x -= 360f;
            if (rotEuler.y > 180f) rotEuler.y -= 360f;

            rotEuler.x = Mathf.Clamp(rotEuler.x, pitchLimit.x, pitchLimit.y);
            rotEuler.y = Mathf.Clamp(rotEuler.y, yawLimit.x, yawLimit.y);

            transform.rotation = rootBone.rotation * Quaternion.Euler(rotEuler);
            
            _viewLimitPlayback += Time.deltaTime * _viewLimitSpeed;
            _viewLimitPlayback = Mathf.Clamp01(_viewLimitPlayback);
        }

        protected void Start()
        {
#if false
            _mainCamera = GetComponent<UnityEngine.Camera>();
            if (_mainCamera != null)
            {
                _fov = _mainCamera.fieldOfView;
            }
#endif
        }

        public void LimitView(Vector2 pitchLimit, Vector2 yawLimit, float blendIn = 1f)
        {
            if (!useViewLimits)
            {
                return;
            }
            
            _cachePitchLimit = _pitchLimit;
            _cacheYawLimit = _yawLimit;
            
            _pitchLimit = pitchLimit;
            _yawLimit = yawLimit;

            _viewLimitSpeed = blendIn;
            _viewLimitPlayback = 0f;
        }

        public void ResetLimit()
        {
            if (!useViewLimits)
            {
                return;
            }
            
            _cachePitchLimit = _pitchLimit;
            _cacheYawLimit = _yawLimit;
            _pitchLimit = _yawLimit = new Vector2(-180f, 90);
        }

        public void UpdateCamera()
        {
            //todo: UpdateViewLimit();
            UpdateShake();
            UpdateFOV();
        }

        public void UpdateCameraAnimation(Quaternion cameraAnimation)
        {
            _animation = cameraAnimation;
        }

        public void PlayShake(CameraShakeInfo shake)
        {
            _shake = shake;
            _target.x = GetRandomTarget(_shake.pitch);
            _target.y = GetRandomTarget(_shake.yaw);
            _target.z = GetRandomTarget(_shake.roll);

            _playBack = 0f;
        }
    }
}