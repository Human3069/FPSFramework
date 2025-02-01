using _KMH_Framework;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Demo.Scripts.Runtime;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Recoil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(CharacterController))]
    public class FPSControllerEx : Demo.Scripts.Runtime.FPSController, IDamageable
    {
        private const string LOG_FORMAT = "<color=white><b>[FPSControllerEx]</b></color> {0}";
        private const float INTERACT_DISTANCE = 2f;

        protected static FPSControllerEx _instance;
        public static FPSControllerEx Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected State _state;
        public State _State
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnStateChanged.Invoke(value);
                }
            }
        }

        public event IDamageable.StateChanged OnStateChanged;

        [SerializeField]
        protected float maxHealth = 100;

        [ReadOnly]
        [SerializeField]
        protected float _currentHealth;
        public float CurrentHealth
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                if (_currentHealth > value)
                {
                    _currentHealth = Mathf.Clamp(value, 0f, maxHealth);

                    if (_currentHealth == 0f)
                    {
                        OnDead();
                    }
                    else
                    {
                        OnDamaged();
                    }
                }
            }
        }

        public void OnDamaged()
        {
          
        }

        public void OnDead()
        {
 
        }

        [HideInInspector]
        public SingleFPSPlayer _SingleFPSPlayer;

        [SerializeField]
        protected Transform weaponParentTransform;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float fireTimeStamp = 0f;

        protected CinemachineVirtualCamera fpsVCam;

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
        public event EquipableValueChanged OnEquipableValueChanged;

        [ReadOnly]
        [SerializeField]
        protected bool _isSeatable;
        public bool IsSeatable
        {
            get
            {
                return _isSeatable;
            }
            protected set
            {
                if (_isSeatable != value)
                {
                    _isSeatable = value;

                    if (OnSeatableValueChanged != null)
                    {
                        OnSeatableValueChanged(value);
                    }
                }
            }
        }

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected bool _isSeated;
        protected bool IsSeated
        {
            get
            {
                return _isSeated;
            }
            set
            {
                if (_isSeated != value)
                {
                    if (value == true)
                    {
                        animator.SetFloat("MoveX", 0f);
                        animator.SetFloat("MoveY", 0f);
                        animator.SetFloat("Velocity", 0f);
                        animator.SetBool("Moving", false);

                        if (CurrentEquipedWeapon != null)
                        {
                            DisableAim();
                            UnequipWeapon();
                            EnableUnarmedState();
                        }
                    }
                    else
                    {
                        EquipWeapon();
                    }

                    _isSeated = value;
                    _characterController.enabled = !IsSeated;
                    movementComponent.enabled = !value;
                }
            }
        }

        [ReadOnly]
        [SerializeField]
        protected Seat currentSeat = null;

        public delegate void SeatableValueChanged(bool isSeatable);
        public event SeatableValueChanged OnSeatableValueChanged;

        protected List<Weapon> weaponList
        {
            get
            {
                return weapons;
            }
        }

        protected WeaponEx _currentEquipedWeapon;
        public WeaponEx CurrentEquipedWeapon
        {
            get
            {
                return _currentEquipedWeapon;
            }
            protected set
            {
                Invoke_OnEquipedWeaponChanged(_currentEquipedWeapon, value);

                _currentEquipedWeapon = value;
            }
        }

        public delegate void EquipedWeaponChnaged(WeaponEx currentWeapon, WeaponEx equipedWeapon);
        public event EquipedWeaponChnaged OnEquipedWeaponChanged;
      
        protected void Invoke_OnEquipedWeaponChanged(WeaponEx currentWeapon, WeaponEx equipedWeapon)
        {
            if (OnEquipedWeaponChanged != null)
            {
                OnEquipedWeaponChanged(currentWeapon ,equipedWeapon);
            }
        }

        protected bool _isAim;
        public bool IsAim
        {
            get
            {
                return _isAim;
            }
            set
            {
                if (_isAim != value)
                {
                    if (CurrentEquipedWeapon != null &&
                        CurrentEquipedWeapon.canAim == true)
                    {
                        _isAim = value;

                        slotLayer.PlayMotion(aimMotionAsset);

                        float alphaThreshold;
                        if (value == true)
                        {
                            aimState = FPSAimState.Aiming;
                            alphaThreshold = 0.5f;
                        }
                        else
                        {
                            aimState = FPSAimState.None;
                            alphaThreshold = 1f;

                            adsLayer.SetPointAim(false);
                        }
                        OnInputAim(value);
                        adsLayer.SetAds(value);
                        swayLayer.SetFreeAimEnable(!value);
                        swayLayer.SetLayerAlpha(alphaThreshold);

                        CurrentEquipedWeapon.IsAiming = value;
                        recoilComponent.isAiming = value;
                    }
                }
            }
        }

        protected CharacterController _characterController;

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            fpsVCam = mainCamera.GetComponent<CinemachineVirtualCamera>();
            _characterController = this.GetComponent<CharacterController>();

            fpsVCam.Priority = 1;

            PostAwake().Forget();
        }

        protected async UniTaskVoid PostAwake()
        {
            await KeyType.Change_FireMode.RegisterEventAsync(OnFireModeChanged);
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void OnEnable()
        {
            CurrentHealth = maxHealth;
        }

        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Time.timeScale = 0.01f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Time.timeScale = 1f;
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
                bool isRayHit = _raycastHit.transform.TryGetComponent<IInteractable>(out IInteractable _interactable);
                bool isInbound = (_raycastHit.transform.position - this.transform.position).magnitude <= INTERACT_DISTANCE;

                bool isTypeEquipable = _interactable is IEquipable;
                bool isTypeSittable = _interactable is ISeatable;

                bool isInteractable = isRayHit && isInbound;
                IsEquipable = isInteractable && isTypeEquipable; // Property
                IsSeatable = isInteractable && isTypeSittable && (IsSeated == false); // Property

                if (KeyType.Interact.IsInputDown() == true)
                {
                    if (IsEquipable == true)
                    {
                        WeaponEx _currentWeapon = CurrentEquipedWeapon;
                        WeaponEx _equipedWeapon = _interactable as WeaponEx;

                        Debug.LogFormat(LOG_FORMAT, "Update(), currentWeapon : " + _currentWeapon + ", _equipedWeapon : " + _equipedWeapon);

                        ChangeWeapon_InternalEx(_equipedWeapon, _currentWeapon, _equipedWeapon.transform.position, _equipedWeapon.transform.rotation);
                    }
                    else if (IsSeatable == true)
                    {
                        IsSeated = true;

                        Seat _seat = _interactable as Seat;
                        _seat.Interact(this.transform, IsSeated, _SingleFPSPlayer);

                        currentSeat = _seat;

                        fpsVCam.Priority = 0;
                    }
                    else if (IsSeated == true)
                    {
                        IsSeated = false;

                        currentSeat.Interact(this.transform, IsSeated, _SingleFPSPlayer);
                        currentSeat = null;

                        fpsVCam.Priority = 1;
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

            CurrentEquipedWeapon.OnFire();
         
            PlayAnimation(CurrentEquipedWeapon.fireClip);
            PlayCameraShake(CurrentEquipedWeapon.cameraShake);

            if (CurrentEquipedWeapon.recoilPattern != null)
            {
                float aimRatio;
                if (IsAiming() == true)
                {
                    aimRatio = CurrentEquipedWeapon.recoilPattern.aimRatio;
                }
                else
                {
                    aimRatio = 1f;
                }

                Vector2 _horizontalVar = CurrentEquipedWeapon.recoilPattern.horizontalVariation;
                float hRecoil = Random.Range(_horizontalVar.x, _horizontalVar.y);

                _controllerRecoil += new Vector2(hRecoil, _recoilStep) * aimRatio;
            }

            if (recoilComponent == null ||
                CurrentEquipedWeapon.weaponAsset.recoilData == null)
            {
                return;
            }

            recoilComponent.Play();
            _recoilStep += CurrentEquipedWeapon.recoilPattern.acceleration;

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
            else if (CurrentEquipedWeapon.CurrentMagCount == 0)
            {
                OnFireReleased();
                return;
            }
            else
            {
                // do nothing
            }

            float fireRate = (60f / CurrentEquipedWeapon.fireRate);
            if (fireTimeStamp >= fireRate)
            {
                fireTimeStamp = 0f;
                Fire();
            }
        }

        protected override void OnFirePressed()
        {
            if (CurrentEquipedWeapon == null ||
                HasActiveAction() == true ||
                IsSeated == true)
            {
                return;
            }

            _bursts = CurrentEquipedWeapon.burstAmount;

            if (CurrentEquipedWeapon.recoilPattern != null)
            {
                _recoilStep = CurrentEquipedWeapon.recoilPattern.step;
            }

            _isFiring = true;
            Fire();
        }

        protected override void OnFireReleased()
        {
            if (CurrentEquipedWeapon == null)
            {
                return;
            }

            if (recoilComponent != null)
            {
                recoilComponent.Stop();
            }

            _recoilStep = 0f;
            _isFiring = false;
            CancelInvoke(nameof(Fire));
        }

        protected override void InitAimPoint(FPSAnimWeapon weapon)
        {
            WeaponEx weaponEx = weapon as WeaponEx;
            fpsAnimator.OnSightChanged(weaponEx.GetAimPoint());

            if (internalAdsLayer != null)
            {
                internalAdsLayer.UpdateAimPoint();
            }
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

            CurrentEquipedWeapon = equipedWeapon;

            InitWeapon(CurrentEquipedWeapon);
            CurrentEquipedWeapon.gameObject.SetActive(true);
            CurrentEquipedWeapon.Initialize();

            if (recoilComponent.fireMode == FireMode.Semi)
            {
                _bursts = CurrentEquipedWeapon.burstAmount;
            }
                
            animator.SetFloat(OverlayType, (float)equipedWeapon.overlayType);
            actionState = FPSActionState.None;
        }

        protected override void InitWeapon(FPSAnimWeapon weapon)
        {
            WeaponEx weaponEx = weapon as WeaponEx;

            recoilComponent.Init(weaponEx.weaponAsset.recoilData, weaponEx.fireRate, weaponEx.CurrentFireMode);

            WeaponTransformData _transformData = weaponEx.weaponTransformData;
            _transformData.aimPoint = weaponEx.AttatchmentHandler.SelectedSight.AimPoint;

            fpsAnimator.OnGunEquipped(weaponEx.weaponAsset, _transformData);
            fpsAnimator.ikRigData.weaponTransform = weaponEx.weaponAsset.weaponBone;

            if (internalLookLayer != null)
            {
                internalLookLayer.SetAimOffsetTable(weaponEx.weaponAsset.aimOffsetTable);
            }

            AnimSequence pose = weaponEx.weaponAsset.overlayPose;
            if (pose == null)
            {
                Debug.LogError("FPSAnimController: OverlayPose is null! Make sure to assign it in the weapon prefab.");
                return;
            }

            fpsAnimator.OnPrePoseSampled();
            PlayPose(weaponEx.weaponAsset.overlayPose);
            fpsAnimator.OnPoseSampled();

            if (fpsCamera != null)
            {
                fpsCamera.cameraData = weaponEx.weaponAsset.adsData.cameraData;
            }
        }

        protected override void UnequipWeapon()
        {
            Debug.LogFormat(LOG_FORMAT, "UnequipWeapon()");

            DisableAim();

            actionState = FPSActionState.WeaponChange;
            GetAnimGraph().GetFirstPersonAnimator().CrossFade(UnEquip, 0.1f);
        }

        protected override void TryReload()
        {
            if (HasActiveAction() == true)
            {
                return;
            }

            AnimSequence reloadClip = CurrentEquipedWeapon.reloadClip;
            if (reloadClip == null)
            {
                return;
            }

            OnFireReleased();

            PlayAnimation(reloadClip);
            CurrentEquipedWeapon.Reload();
            actionState = FPSActionState.Reloading;

            if (CurrentEquipedWeapon.projectileType == Pool.ProjectileType._Musket_Bullet ||
                CurrentEquipedWeapon.projectileType == Pool.ProjectileType._577_450_SR_Bullet)
            {
                IsAim = false;
            }
        }

        protected override void TryGrenadeThrow()
        {
            if (HasActiveAction() == true)
            {
                return;
            }

            if (CurrentEquipedWeapon == null ||
                CurrentEquipedWeapon.grenadeClip == null)
            {
                return;
            }

            OnFireReleased();
            DisableAim();
            PlayAnimation(CurrentEquipedWeapon.grenadeClip);
            actionState = FPSActionState.Reloading;
        }

        protected void ChangeWeapon_InternalEx(WeaponEx equipedWeapon, WeaponEx currentWeapon, Vector3 equipedWeaponPos, Quaternion equipedWeaponRot)
        {
            Debug.LogFormat(LOG_FORMAT, "ChangeWeapon_Internal(), equipedWeapon : " + equipedWeapon + ", currentWeapon : " + currentWeapon);

            if (movementComponent.PoseState == FPSPoseState.Prone ||
                HasActiveAction() == true)
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

        protected virtual void OnFireModeChanged(bool isOn)
        {
            if (isOn == true && recoilComponent != null && CurrentEquipedWeapon != null)
            {
                CurrentEquipedWeapon?.MoveNextFireMode();
                recoilComponent.fireMode = CurrentEquipedWeapon.CurrentFireMode;
            }
        }

        public void UpdateMouseShowState(bool isShowable)
        {
            if (isShowable == true)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                aimState = FPSAimState.Ready;
                lookLayer.SetLayerAlpha(0.5f);
                OnFireReleased();
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                aimState = FPSAimState.None;
                lookLayer.SetLayerAlpha(1f);
            }
        }

        #region #Movement
        protected override void UpdateActionInput()
        {
            if (movementComponent.MovementState == FPSMovementState.Sprinting ||
                KeyCodeManager.Instance == null)
            {
                return;
            }

            if (KeyType.Reload.IsInput() == true)
            {
                TryReload();
            }

            if (KeyType.Throw_Grenade.IsInput() == true)
            {
                TryGrenadeThrow();
            }

            if (aimState != FPSAimState.Ready)
            {
                bool wasLeaning = _isLeaning;

                bool isLeanRight = KeyType.Lean_Right.IsInput();
                bool isLeanLeft = KeyType.Lean_Left.IsInput();

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
            
                if (Input.GetKey(KeyCode.Mouse0) &&
                    CurrentEquipedWeapon != null)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0) &&
                        CurrentEquipedWeapon.CurrentMagCount > 0)
                    {
                        OnFirePressed();
                    }

                    if (CurrentEquipedWeapon != null)
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
                    IsAim = !IsAim;
                }

                if (Input.GetKeyDown(KeyCode.V))
                {
                    ChangeScope();
                }
            }

#if false
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
#endif
        }

        protected override void UpdateLookInput()
        {
            if (Cursor.visible == true)
            {
                return;
            }

            _freeLook = Input.GetKey(KeyCode.X);

            float deltaMouseX = Input.GetAxis("Mouse X") * FPSManager.Instance.ActualSenstivity;
            float deltaMouseY = -Input.GetAxis("Mouse Y") * FPSManager.Instance.ActualSenstivity;

            if (_freeLook == true)
            {
                // No input for both controller and animation component. We only want to rotate the camera
                _freeLookInput.x += deltaMouseX;
                _freeLookInput.y += deltaMouseY;

                _freeLookInput.x = Mathf.Clamp(_freeLookInput.x, -freeLookAngle.x, freeLookAngle.x);
                _freeLookInput.y = Mathf.Clamp(_freeLookInput.y, -freeLookAngle.y, freeLookAngle.y);

                return;
            }

            _freeLookInput = Vector2.Lerp(_freeLookInput, Vector2.zero, FPSAnimLib.ExpDecayAlpha(15f, Time.deltaTime));

            _playerInput.x += deltaMouseX;
            _playerInput.y += deltaMouseY;

            float proneWeight = animator.GetFloat("ProneWeight");
            Vector2 pitchClamp = Vector2.Lerp(new Vector2(-90f, 90f), new Vector2(-30, 0f), proneWeight);

            _playerInput.y = Mathf.Clamp(_playerInput.y, pitchClamp.x, pitchClamp.y);
            moveRotation *= Quaternion.Euler(0f, deltaMouseX, 0f);
            TurnInPlace();

            _jumpState = Mathf.Lerp(_jumpState, movementComponent.IsInAir() ? 1f : 0f,
                FPSAnimLib.ExpDecayAlpha(10f, Time.deltaTime));

            float moveWeight = Mathf.Clamp01(movementComponent.AnimatorVelocity.magnitude);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, moveRotation, moveWeight);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, moveRotation, _jumpState);
            _playerInput.x *= 1f - moveWeight;
            _playerInput.x *= 1f - _jumpState;

            charAnimData.SetAimInput(_playerInput);
            charAnimData.AddDeltaInput(new Vector2(deltaMouseX, charAnimData.deltaAimInput.y));
        }

        protected override void OnSprintStarted()
        {
            OnFireReleased();
            lookLayer.SetLayerAlpha(0.5f);
            adsLayer.SetLayerAlpha(0f);

            if (CurrentEquipedWeapon != null)
            {
                if (CurrentEquipedWeapon.overlayType == Demo.Scripts.Runtime.OverlayType.Rifle)
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

        public void ReturnPool()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}