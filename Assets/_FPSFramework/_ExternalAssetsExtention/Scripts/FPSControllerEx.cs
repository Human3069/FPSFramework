using _KMH_Framework;
using Demo.Scripts.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSFramework
{
    public class FPSControllerEx : FPSController
    {
        private const string LOG_FORMAT = "<color=white><b>[FPSControllerEx]</b></color> {0}";
        private const float INTERACT_DISTANCE = 2f;

        [SerializeField]
        protected Transform weaponParentTransform;

        protected Camera _fpsCamera;

        protected List<Weapon> weaponList
        {
            get
            {
                return weapons;
            }
        }

        protected Dictionary<string, KeySetting> _keyData
        {
            get
            {
                return KeyInputManager.Instance.KeyData;
            }
        }

        protected void Awake()
        {
            _fpsCamera = mainCamera.GetComponent<Camera>();
        }

        protected override void Update()
        {
            // base.Update();
            Time.timeScale = timeScale;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit(0);
            }

            UpdateActionInput();
            UpdateLookInput();
            UpdateRecoil();

            charAnimData.moveInput = movementComponent.AnimatorVelocity;
            if (weaponList.Count > 0 &&
                actionState != FPSActionState.WeaponChange)
            {
                UpdateAnimController();
            }

            // Interact
            Ray ray = new Ray(mainCamera.position, mainCamera.forward * INTERACT_DISTANCE);
            if (Physics.Raycast(ray, out RaycastHit _raycastHit) == true)
            {
                if (_raycastHit.transform.TryGetComponent<IEquipable>(out IEquipable _equipable) == true)
                {
                    if (_keyData[KeyInputManager.KEY_INTERACT].IsInputDown == true)
                    {
                        WeaponEx _weapon = _equipable as WeaponEx;
                        _weapon.DoInteract();
                        _weapon.transform.parent = weaponParentTransform;
                        _weapon.transform.localPosition = Vector3.zero;
                        _weapon.transform.localEulerAngles = Vector3.zero;

                        weaponList.Add(_weapon);

                        ChangeWeapon_Internal();
                    }
                }
            }
        }

        protected override void UnequipWeapon()
        {
            Debug.LogFormat(LOG_FORMAT, "UnequipWeapon()");

            DisableAim();

            actionState = FPSActionState.WeaponChange;
            GetAnimGraph().GetFirstPersonAnimator().CrossFade(UnEquip, 0.1f);
        }

        protected IEnumerator EquipWeaponEx(WeaponEx currentWeapon)
        {
            Debug.LogFormat(LOG_FORMAT, "EquipWeaponEx(), currentWeapon.name : " + currentWeapon);

            yield return new WaitForSeconds(equipDelay);

            if (currentWeapon != null)
            {
                currentWeapon.Release(this.transform.position + (this.transform.forward * 0.5f));
            }
            if (weaponList.Count == 0)
            {
                yield break;
            }

            weaponList[_lastIndex].gameObject.SetActive(false);
            WeaponEx _weaponEx = weaponList[_index] as WeaponEx;

            _bursts = _weaponEx.burstAmount;

            InitWeapon(_weaponEx);
            _weaponEx.Initialize();
            _weaponEx.gameObject.SetActive(true);
            Debug.LogFormat(LOG_FORMAT, "_weaponEx.name : " + _weaponEx.name);

            animator.SetFloat(OverlayType, (float)_weaponEx.overlayType);
            actionState = FPSActionState.None;
        }

        protected override void ChangeWeapon_Internal()
        {
            Debug.LogFormat(LOG_FORMAT, "ChangeWeapon_Internal()");

            if (movementComponent.PoseState == FPSPoseState.Prone)
            {
                return;
            }

            if (HasActiveAction() == true)
            {
                return;
            }

            OnFireReleased();

            WeaponEx currentWeapon = GetGun() as WeaponEx;
            int newIndex = _index;
            newIndex++;
            if (newIndex > weapons.Count - 1)
            {
                newIndex = 0;
            }

            _lastIndex = _index;
            _index = newIndex;

            UnequipWeapon();
            StartCoroutine(EquipWeaponEx(currentWeapon));
        }

        public override void ToggleAim()
        {
            if (weaponList.Count == 0)
            {
                return;
            }

            if (GetGun().canAim == false)
            {
                return;
            }

            slotLayer.PlayMotion(aimMotionAsset);

            if (IsAiming() == false)
            {
                aimState = FPSAimState.Aiming;
                OnInputAim(true);

                adsLayer.SetAds(true);
                swayLayer.SetFreeAimEnable(false);
                swayLayer.SetLayerAlpha(0.5f);
            }
            else
            {
                DisableAim();
            }

            recoilComponent.isAiming = IsAiming();
        }

        #region #Movement
        protected override void UpdateActionInput()
        {
            if (movementComponent.MovementState == FPSMovementState.Sprinting ||
                KeyInputManager.Instance == null ||
                KeyInputManager.Instance.IsInitialized == false)
            {
                return;
            }

            if (_keyData[KeyInputManager.KEY_RELOAD].IsInputDown == true)
            {
                TryReload();
            }

            if (_keyData[KeyInputManager.KEY_THROW_GRENADE].IsInputDown == true)
            {
                TryGrenadeThrow();
            }

            if (aimState != FPSAimState.Ready)
            {
                bool wasLeaning = _isLeaning;

                bool isLeanRight = _keyData[KeyInputManager.KEY_LEAN_RIGHT].isInput;
                bool isLeanLeft = _keyData[KeyInputManager.KEY_LEAN_LEFT].isInput;

                _isLeaning = (isLeanRight == true) ||
                             (isLeanLeft == true);

                if (_isLeaning != wasLeaning)
                {
                    slotLayer.PlayMotion(leanMotionAsset);

                    float targetValue;
                    if (wasLeaning == true)
                    {
                        targetValue = 0f;
                    }
                    else
                    {
                        if (isLeanRight == true)
                        {
                            targetValue = -startLean;
                        }
                        else
                        {
                            targetValue = startLean;
                        }
                    }
                    charAnimData.SetLeanInput(targetValue);
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

                if (Input.GetKeyDown(KeyCode.B) && IsAiming() == true)
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

            if (Input.GetKeyDown(KeyCode.H) == true)
            {
                if (aimState == FPSAimState.Ready)
                {
                    aimState = FPSAimState.None;
                    lookLayer.SetLayerAlpha(1f);
                }
                else
                {
                    aimState = FPSAimState.Ready;
                    lookLayer.SetLayerAlpha(0.5f);
                    OnFireReleased();
                }
            }
        }

        protected override void OnSprintStarted()
        {
            OnFireReleased();
            lookLayer.SetLayerAlpha(0.5f);
            adsLayer.SetLayerAlpha(0f);

            if (weaponList.Count > 0)
            {
                if (GetGun().overlayType == Demo.Scripts.Runtime.OverlayType.Rifle)
                {
                    locoLayer.BlendInIkPose(sprintPose);
                }
            }

            aimState = FPSAimState.None;

            if (recoilComponent != null)
            {
                recoilComponent.Stop();
            }
        }
        #endregion
    }
}