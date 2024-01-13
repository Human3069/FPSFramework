using _KMH_Framework;
using Demo.Scripts.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FPSFramework
{
    public class FPSControllerEx : FPSController
    {
        protected Dictionary<string, KeySetting> _keyData
        {
            get
            {
                return KeyInputManager.Instance.KeyData;
            }
        }

        protected override void UpdateActionInput()
        {
            if (movementComponent.MovementState == FPSMovementState.Sprinting ||
                KeyInputManager.Instance == null ||
                KeyInputManager.Instance.IsInitialized == false)
            {
                return;
            }

            if (_keyData[KeyInputManager.KEY_RELOAD].isInput == true)
            {
                TryReload();
            }

            if (_keyData[KeyInputManager.KEY_THROW_GRENADE].isInput == true)
            {
                TryGrenadeThrow();
            }

            if (_keyData[KeyInputManager.KEY_INTERACT].isInput == true)
            {
                ChangeWeapon_Internal();
            }

            if (aimState != FPSAimState.Ready)
            {
                bool wasLeaning = _isLeaning;

                bool isLeanRight = _keyData[KeyInputManager.KEY_LEAN_RIGHT].isInput;
                bool isLeanLeft = _keyData[KeyInputManager.KEY_LEAN_LEFT].isInput;

                _isLeaning = (isLeanRight || isLeanLeft);

                if (_isLeaning != wasLeaning)
                {
                    slotLayer.PlayMotion(leanMotionAsset);
                    charAnimData.SetLeanInput(wasLeaning ? 0f : isLeanRight ? -startLean : startLean);
                }

                if (_isLeaning == true)
                {
                    float leanValue = Input.GetAxis("Mouse ScrollWheel") * smoothLeanStep;
                    charAnimData.AddLeanInput(leanValue);
                }

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    OnFirePressed();
                }

                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    OnFireReleased();
                }

                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    ToggleAim();
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    ChangeScope();
                }

                if (Input.GetKeyDown(KeyCode.B) && IsAiming())
                {
                    if (aimState == FPSAimState.PointAiming)
                    {
                        adsLayer.SetPointAim(false);
                        aimState = FPSAimState.Aiming;
                    }
                    else
                    {
                        adsLayer.SetPointAim(true);
                        aimState = FPSAimState.PointAiming;
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (aimState == FPSAimState.Ready)
                {
                    aimState = FPSAimState.None;
                    lookLayer.SetLayerAlpha(1f);
                }
                else
                {
                    aimState = FPSAimState.Ready;
                    lookLayer.SetLayerAlpha(.5f);
                    OnFireReleased();
                }
            }
        }
    }
}