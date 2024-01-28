using _KMH_Framework;
using Demo.Scripts.Runtime;
using Kinemation.FPSFramework.Runtime.Recoil;
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

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float fireTimeStamp = 0f;

        protected Camera _fpsCamera;

        protected bool _isEquipable;
        public bool IsEquipable
        {
            get
            {
                return _isEquipable;
            }
            protected set
            {
                if (_isEquipable != value)
                {
                    _isEquipable = value;

                    if (OnEquipableValueChanged != null)
                    {
                        OnEquipableValueChanged(value);
                    }
                }
            }
        }

        public delegate void EquipableValueChanged(bool isEquipable);
        public static event EquipableValueChanged OnEquipableValueChanged;

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

        protected WeaponEx currentEquipedWeapon;

        protected void Awake()
        {
            _fpsCamera = mainCamera.GetComponent<Camera>();
        }

        protected override void Update()
        {
            // Debug.Log("weaponList.Count : " + weaponList.Count + ", _index : " + _index + ", lastIndex : " + _lastIndex);
            // base.Update();

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Time.timeScale = 0.01f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Time.timeScale = 1f;
            }

            // Time.timeScale = timeScale;
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
            Ray ray = new Ray(mainCamera.position, mainCamera.forward);
            if (Physics.Raycast(ray, out RaycastHit _raycastHit) == true)
            {
                bool isRayHit = _raycastHit.transform.TryGetComponent<IEquipable>(out IEquipable _equipable);
                bool isInbound = (_raycastHit.transform.position - this.transform.position).magnitude <= INTERACT_DISTANCE;

                IsEquipable = isRayHit && isInbound;
                if (IsEquipable == true)
                {
                    if (_keyData[KeyInputManager.KEY_INTERACT].IsInputDown == true)
                    {
                        WeaponEx _currentWeapon = GetGun() as WeaponEx;
                        WeaponEx _equipedWeapon = _equipable as WeaponEx;

                        Debug.LogFormat(LOG_FORMAT, "Update(), currentWeapon : " + _currentWeapon + ", _equipedWeapon : " + _equipedWeapon);

                        ChangeWeapon_InternalEx(_equipedWeapon, _currentWeapon, _equipedWeapon.transform.position, _equipedWeapon.transform.rotation);
                    }
                }
            }
        }

        protected override void Fire()
        {
            if (HasActiveAction() == true)
            {
                return;
            }

            currentEquipedWeapon.OnFire();
         
            PlayAnimation(currentEquipedWeapon.fireClip);
            PlayCameraShake(currentEquipedWeapon.cameraShake);

            if (currentEquipedWeapon.recoilPattern != null)
            {
                float aimRatio;
                if (IsAiming() == true)
                {
                    aimRatio = currentEquipedWeapon.recoilPattern.aimRatio;
                }
                else
                {
                    aimRatio = 1f;
                }

                Vector2 _horizontalVar = currentEquipedWeapon.recoilPattern.horizontalVariation;
                float hRecoil = UnityEngine.Random.Range(_horizontalVar.x, _horizontalVar.y);

                _controllerRecoil += new Vector2(hRecoil, _recoilStep) * aimRatio;
            }

            if (recoilComponent == null ||
                currentEquipedWeapon.weaponAsset.recoilData == null)
            {
                return;
            }

            recoilComponent.Play();
            _recoilStep += currentEquipedWeapon.recoilPattern.acceleration;

            if (recoilComponent.fireMode == FireMode.Burst)
            {
                _bursts--;
            }
        }

        protected void OnFirePressed_Constantly()
        {
            if (recoilComponent.fireMode == FireMode.Semi)
            {
                _isFiring = false;
                return;
            }
            else if (recoilComponent.fireMode == FireMode.Burst)
            {
                if (_bursts == 0)
                {
                    OnFireReleased();
                    return;
                }
            }
            else
            {
                // do nothing
            }

            float fireRate = (60f / currentEquipedWeapon.fireRate);
            if (fireTimeStamp >= fireRate)
            {
                fireTimeStamp = 0f;
                Fire();
            }
        }

        protected override void OnFirePressed()
        {
            Debug.LogFormat(LOG_FORMAT, "OnFirePressed()");

            if (currentEquipedWeapon == null ||
                HasActiveAction() == true)
            {
                return;
            }

            _bursts = currentEquipedWeapon.burstAmount;

            if (currentEquipedWeapon.recoilPattern != null)
            {
                _recoilStep = currentEquipedWeapon.recoilPattern.step;
            }

            _isFiring = true;
            Fire();
        }

        protected override void OnFireReleased()
        {
            Debug.LogFormat(LOG_FORMAT, "OnFireReleased()");

            if (weapons.Count == 0) return;

            if (recoilComponent != null)
            {
                recoilComponent.Stop();
            }

            _recoilStep = 0f;
            _isFiring = false;
            CancelInvoke(nameof(Fire));
        }

        protected IEnumerator EquipWeaponEx(WeaponEx equipedWeapon, WeaponEx currentWeapon, Vector3 equipedWeaponPos, Quaternion equipedWeaponRot)
        {
            Debug.LogFormat(LOG_FORMAT, "EquipWeaponEx(), _equipedWeapon : " + equipedWeapon + ", currentWeapon : " + currentWeapon);

            equipedWeapon.DoInteract();
            equipedWeapon.transform.parent = weaponParentTransform;
            equipedWeapon.transform.localPosition = Vector3.zero;
            equipedWeapon.transform.localEulerAngles = Vector3.zero;

            yield return new WaitForSeconds(equipDelay);

            if (currentWeapon != null)
            {
                currentWeapon.Release(equipedWeaponPos, equipedWeaponRot);
                currentWeapon.transform.parent = null;

                weaponList.Remove(currentWeapon);
                _index = 0;
            }
            if (weaponList.Count == 0)
            {
                yield break;
            }

            currentEquipedWeapon = equipedWeapon;

            InitWeapon(currentEquipedWeapon);
            currentEquipedWeapon.Initialize();
            currentEquipedWeapon.gameObject.SetActive(true);

            if (recoilComponent.fireMode == FireMode.Semi)
            {
                _bursts = currentEquipedWeapon.burstAmount;
            }
                
            animator.SetFloat(OverlayType, (float)equipedWeapon.overlayType);
            actionState = FPSActionState.None;
        }

        protected override void UnequipWeapon()
        {
            Debug.LogFormat(LOG_FORMAT, "UnequipWeapon()");

            DisableAim();

            actionState = FPSActionState.WeaponChange;
            GetAnimGraph().GetFirstPersonAnimator().CrossFade(UnEquip, 0.1f);
        }

        protected void ChangeWeapon_InternalEx(WeaponEx equipedWeapon, WeaponEx currentWeapon, Vector3 equipedWeaponPos, Quaternion equipedWeaponRot)
        {
            Debug.LogFormat(LOG_FORMAT, "ChangeWeapon_Internal(), equipedWeapon : " + equipedWeapon + ", currentWeapon : " + currentWeapon);

            if (movementComponent.PoseState == FPSPoseState.Prone)
            {
                return;
            }

            if (HasActiveAction() == true)
            {
                return;
            }

            weaponList.Add(equipedWeapon);

            OnFireReleased();

            int newIndex = _index;
            newIndex++;
            if (newIndex > weapons.Count - 1)
            {
                newIndex = 0;
            }

            _lastIndex = _index;
            _index = newIndex;

            UnequipWeapon();
            StartCoroutine(EquipWeaponEx(equipedWeapon, currentWeapon, equipedWeaponPos, equipedWeaponRot));
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
            
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        OnFirePressed();
                    }

                    if (currentEquipedWeapon != null)
                    {
                        OnFirePressed_Constantly();
                    }

                    fireTimeStamp += Time.deltaTime;
                }

                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    OnFireReleased();

                    fireTimeStamp = 0f;
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