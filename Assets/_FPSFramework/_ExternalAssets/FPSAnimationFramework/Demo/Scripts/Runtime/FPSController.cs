// Designed by KINEMATION, 2023

using System;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;

using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Demo.Scripts.Runtime
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class TabAttribute : PropertyAttribute
    {
        public readonly string tabName;

        public TabAttribute(string tabName)
        {
            this.tabName = tabName;
        }
    }
    
    public enum FPSAimState
    {
        None,
        Ready,
        Aiming,
        PointAiming
    }

    public enum FPSActionState
    {
        None,
        Reloading,
        WeaponChange
    }

    // An example-controller class
    public class FPSController : FPSAnimController
    {
        private const string LOG_FORMAT = "<color=white><b>[FPSController]</b></color> {0}";

        [Tab("Animation")] 
        [Header("General")] 
        [SerializeField] protected Animator animator;

        [Header("Turn In Place")]
        [SerializeField] protected float turnInPlaceAngle;
        [SerializeField] protected AnimationCurve turnCurve = new AnimationCurve(new Keyframe(0f, 0f));
        [SerializeField] protected float turnSpeed = 1f;

        [Header("Leaning")] 
        [SerializeField] protected float smoothLeanStep = 1f;
        [SerializeField, Range(0f, 1f)] protected float startLean = 1f;
        
        [Header("Dynamic Motions")]
        [SerializeField] protected IKAnimation aimMotionAsset;
        [SerializeField] protected IKAnimation leanMotionAsset;
        [SerializeField] protected IKAnimation crouchMotionAsset;
        [SerializeField] protected IKAnimation unCrouchMotionAsset;
        [SerializeField] protected IKAnimation onJumpMotionAsset;
        [SerializeField] protected IKAnimation onLandedMotionAsset;
        [SerializeField] protected IKAnimation onStartStopMoving;
        
        [SerializeField] protected IKPose sprintPose;
        [SerializeField] protected IKPose pronePose;

        // Animation Layers
        [SerializeField] [HideInInspector] protected LookLayer lookLayer;
        [SerializeField] [HideInInspector] protected AdsLayer adsLayer;
        [SerializeField] [HideInInspector] protected SwayLayer swayLayer;
        [SerializeField] [HideInInspector] protected LocomotionLayer locoLayer;
        [SerializeField] [HideInInspector] protected SlotLayer slotLayer;
        [SerializeField] [HideInInspector] protected WeaponCollision collisionLayer;
        // Animation Layers
        
        [Header("General")] 
        [Tab("Controller")]
        [SerializeField] protected float timeScale = 1f;
        [SerializeField, Min(0f)] protected float equipDelay = 0f;

        [Header("Camera")]
        [SerializeField] protected Transform mainCamera;
        [SerializeField] protected Transform cameraHolder;
        [SerializeField] protected Transform firstPersonCamera;
        [SerializeField] protected float sensitivity;
        [SerializeField] protected Vector2 freeLookAngle;

        [Header("Movement")] 
        [SerializeField] protected FPSMovement movementComponent;
        
        [SerializeField] [Tab("Weapon")] 
        protected List<Weapon> weapons;
        protected Vector2 _playerInput;

        // Used for free-look
        protected Vector2 _freeLookInput;

        protected int _index;
        protected int _lastIndex;
        
        protected int _bursts;
        protected bool _freeLook;
        
        protected FPSAimState aimState;
        protected FPSActionState actionState;
        
        protected static readonly int Crouching = Animator.StringToHash("Crouching");
        protected static readonly int OverlayType = Animator.StringToHash("OverlayType");
        protected static readonly int TurnRight = Animator.StringToHash("TurnRight");
        protected static readonly int TurnLeft = Animator.StringToHash("TurnLeft");
        protected static readonly int UnEquip = Animator.StringToHash("UnEquip");

        protected Vector2 _controllerRecoil;
        protected float _recoilStep;
        protected bool _isFiring;

        protected bool _isUnarmed;

        protected void InitLayers()
        {
            InitAnimController();
            
            animator = GetComponentInChildren<Animator>();
            lookLayer = GetComponentInChildren<LookLayer>();
            adsLayer = GetComponentInChildren<AdsLayer>();
            locoLayer = GetComponentInChildren<LocomotionLayer>();
            swayLayer = GetComponentInChildren<SwayLayer>();
            slotLayer = GetComponentInChildren<SlotLayer>();
            collisionLayer = GetComponentInChildren<WeaponCollision>();
        }

        protected bool HasActiveAction()
        {
            return actionState != FPSActionState.None;
        }

        public bool IsAiming()
        {
            return aimState is FPSAimState.Aiming or FPSAimState.PointAiming;
        }

        protected void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            moveRotation = transform.rotation;

            movementComponent = GetComponent<FPSMovement>();
            
            movementComponent.onStartMoving.AddListener(OnMoveStarted);
            movementComponent.onStopMoving.AddListener(OnMoveEnded);
            
            movementComponent.onCrouch.AddListener(OnCrouch);
            movementComponent.onUncrouch.AddListener(OnUncrouch);
            
            movementComponent.onJump.AddListener(OnJump);
            movementComponent.onLanded.AddListener(OnLand);
            
            movementComponent.onSprintStarted.AddListener(OnSprintStarted);
            movementComponent.onSprintEnded.AddListener(OnSprintEnded);
            
            movementComponent.onSlideStarted.AddListener(OnSlideStarted);
            movementComponent.onSlideEnded.AddListener(OnSlideEnded);
            
            movementComponent.onProneStarted.AddListener(() => collisionLayer.SetLayerAlpha(0f));
            movementComponent.onProneEnded.AddListener(() => collisionLayer.SetLayerAlpha(1f));

            movementComponent.slideCondition += () => !HasActiveAction();
            movementComponent.sprintCondition += () => !HasActiveAction();
            movementComponent.proneCondition += () => !HasActiveAction();

            actionState = FPSActionState.None;

            InitLayers();
            EquipWeapon();
        }
        
        protected virtual void UnequipWeapon()
        {
            DisableAim();

            actionState = FPSActionState.WeaponChange;
            GetAnimGraph().GetFirstPersonAnimator().CrossFade(UnEquip, 0.1f);
        }

        public void ResetActionState()
        {
            actionState = FPSActionState.None;
        }

        public void RefreshStagedState()
        {
        }
        
        public void ResetStagedState()
        {
        }

        protected virtual void EquipWeapon()
        {
            Debug.LogFormat(LOG_FORMAT, "EquipWeapon()");

            if (weapons.Count == 0) return;

            weapons[_lastIndex].gameObject.SetActive(false);
            var gun = weapons[_index];

            _bursts = gun.burstAmount;
            
            //StopAnimation(0.1f);
            InitWeapon(gun);
            gun.gameObject.SetActive(true);

            animator.SetFloat(OverlayType, (float) gun.overlayType);
            actionState = FPSActionState.None;
        }

        protected void EnableUnarmedState()
        {
            Debug.LogFormat(LOG_FORMAT, "EnableUnarmedState()");

            if (weapons.Count == 0) return;
            
            weapons[_index].gameObject.SetActive(false);
            animator.SetFloat(OverlayType, 0);
        }
        
        protected virtual void ChangeWeapon_Internal()
        {
            Debug.LogFormat(LOG_FORMAT, "ChangeWeapon_Internal()");

            if (movementComponent.PoseState == FPSPoseState.Prone) return;
            
            if (HasActiveAction()) return;
            
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
            Invoke(nameof(EquipWeapon), equipDelay);
        }

        public void DisableAim()
        {
            if (!GetGun().canAim) return;
            
            aimState = FPSAimState.None;
            OnInputAim(false);
            
            adsLayer.SetAds(false);
            adsLayer.SetPointAim(false);
            swayLayer.SetFreeAimEnable(true);
            swayLayer.SetLayerAlpha(1f);
        }

        public virtual void ToggleAim()
        {
            if (!GetGun().canAim) return;
            
            slotLayer.PlayMotion(aimMotionAsset);
            
            if (!IsAiming())
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

        public void ChangeScope()
        {
            InitAimPoint(GetGun());
        }

        protected virtual void Fire()
        {
            if (HasActiveAction()) return;
            
            GetGun().OnFire();
            PlayAnimation(GetGun().fireClip);
            
            PlayCameraShake(GetGun().cameraShake);

            if (GetGun().recoilPattern != null)
            {
                float aimRatio = IsAiming() ? GetGun().recoilPattern.aimRatio : 1f;
                float hRecoil = Random.Range(GetGun().recoilPattern.horizontalVariation.x,
                    GetGun().recoilPattern.horizontalVariation.y);
                _controllerRecoil += new Vector2(hRecoil, _recoilStep) * aimRatio;
            }
            
            if (recoilComponent == null || GetGun().weaponAsset.recoilData == null)
            {
                return;
            }
            
            recoilComponent.Play();
            
            if (recoilComponent.fireMode == FireMode.Burst)
            {
                if (_bursts == 0)
                {
                    OnFireReleased();
                    return;
                }
                
                _bursts--;
            }

            if (recoilComponent.fireMode == FireMode.Semi)
            {
                _isFiring = false;
                return;
            }
            
            Invoke(nameof(Fire), 60f / GetGun().fireRate);
            _recoilStep += GetGun().recoilPattern.acceleration;
        }

        protected virtual void OnFirePressed()
        {
            Debug.LogFormat(LOG_FORMAT, "OnFirePressed()");

            if (weapons.Count == 0 || HasActiveAction()) return;

            _bursts = GetGun().burstAmount - 1;

            if (GetGun().recoilPattern != null)
            {
                _recoilStep = GetGun().recoilPattern.step;
            }
            
            _isFiring = true;
            Fire();
        }

        protected Weapon GetGun()
        {
            if (weapons.Count == 0) return null;
            
            return weapons[_index];
        }

        protected virtual void OnFireReleased()
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

        protected void OnMoveStarted()
        {
            if (slotLayer != null)
            {
                slotLayer.PlayMotion(onStartStopMoving);
            }

            if (movementComponent.PoseState == FPSPoseState.Prone)
            {
                locoLayer.BlendInIkPose(pronePose);
            }
        }

        protected void OnMoveEnded()
        {
            if (slotLayer != null)
            {
                slotLayer.PlayMotion(onStartStopMoving);
            }
            
            if (movementComponent.PoseState == FPSPoseState.Prone)
            {
                locoLayer.BlendOutIkPose();
            }
        }

        protected void OnSlideStarted()
        {
            lookLayer.SetLayerAlpha(0.3f);
            GetAnimGraph().GetBaseAnimator().CrossFade("Sliding", 0.1f);
        }

        protected void OnSlideEnded()
        {
            lookLayer.SetLayerAlpha(1f);
        }

        protected virtual void OnSprintStarted()
        {
            OnFireReleased();
            lookLayer.SetLayerAlpha(0.5f);
            adsLayer.SetLayerAlpha(0f);

            if (GetGun().overlayType == Runtime.OverlayType.Rifle)
            {
                locoLayer.BlendInIkPose(sprintPose);
            }
            
            aimState = FPSAimState.None;

            if (recoilComponent != null)
            {
                recoilComponent.Stop();
            }
        }

        protected void OnSprintEnded()
        {
            lookLayer.SetLayerAlpha(1f);
            adsLayer.SetLayerAlpha(1f);
            locoLayer.BlendOutIkPose();
        }

        protected void OnCrouch()
        {
            lookLayer.SetPelvisWeight(0f);
            animator.SetBool(Crouching, true);
            slotLayer.PlayMotion(crouchMotionAsset);
            
            GetAnimGraph().GetFirstPersonAnimator().SetFloat("MovementPlayRate", .7f);
        }

        protected void OnUncrouch()
        {
            lookLayer.SetPelvisWeight(1f);
            animator.SetBool(Crouching, false);
            slotLayer.PlayMotion(unCrouchMotionAsset);
            
            GetAnimGraph().GetFirstPersonAnimator().SetFloat("MovementPlayRate", 1f);
        }

        protected void OnJump()
        {
            slotLayer.PlayMotion(onJumpMotionAsset);
        }

        protected void OnLand()
        {
            slotLayer.PlayMotion(onLandedMotionAsset);
        }

        protected virtual void TryReload()
        {
            if (HasActiveAction()) return;

            var reloadClip = GetGun().reloadClip;

            if (reloadClip == null) return;
            
            OnFireReleased();
            
            PlayAnimation(reloadClip);
            GetGun().Reload();
            actionState = FPSActionState.Reloading;
        }

        protected void TryGrenadeThrow()
        {
            if (HasActiveAction()) return;
            if (GetGun().grenadeClip == null) return;
            
            OnFireReleased();
            DisableAim();
            PlayAnimation(GetGun().grenadeClip);
            actionState = FPSActionState.Reloading;
        }

        protected bool _isLeaning;

        protected virtual void UpdateActionInput()
        {
            if (movementComponent.MovementState == FPSMovementState.Sprinting)
            {
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                TryReload();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                TryGrenadeThrow();
            }
            
            if (Input.GetKeyDown(KeyCode.F))
            {
                ChangeWeapon_Internal();
            }
            
            if (aimState != FPSAimState.Ready)
            {
                bool wasLeaning = _isLeaning;
                
                bool rightLean = Input.GetKey(KeyCode.E);
                bool leftLean = Input.GetKey(KeyCode.Q);

                _isLeaning = rightLean || leftLean;
                
                if (_isLeaning != wasLeaning)
                {
                    slotLayer.PlayMotion(leanMotionAsset);
                    charAnimData.SetLeanInput(wasLeaning ? 0f : rightLean ? -startLean : startLean);
                }

                if (_isLeaning == true)
                {
                    float leanValue = Input.GetAxis("Mouse ScrollWheel") * smoothLeanStep;
                    charAnimData.AddLeanInput(leanValue);
                }

                if (Input.GetKeyDown(KeyCode.Mouse0) == true)
                {
                    OnFirePressed();
                }

                if (Input.GetKeyUp(KeyCode.Mouse0) == true)
                {
                    OnFireReleased();
                }

                if (Input.GetKeyDown(KeyCode.Mouse1) == true)
                {
                    ToggleAim();
                }

                if (Input.GetKeyDown(KeyCode.V) == true)
                {
                    ChangeScope();
                }

                if (Input.GetKeyDown(KeyCode.B) &&
                    IsAiming() == true)
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
                    lookLayer.SetLayerAlpha(.5f);
                    OnFireReleased();
                }
            }
        }

        protected Quaternion desiredRotation;
        protected Quaternion moveRotation;
        protected float turnProgress = 1f;
        protected bool isTurning = false;

        protected void TurnInPlace()
        {
            float turnInput = _playerInput.x;
            _playerInput.x = Mathf.Clamp(_playerInput.x, -90f, 90f);
            turnInput -= _playerInput.x;

            float sign = Mathf.Sign(_playerInput.x);
            if (Mathf.Abs(_playerInput.x) > turnInPlaceAngle)
            {
                if (!isTurning)
                {
                    turnProgress = 0f;
                    
                    animator.ResetTrigger(TurnRight);
                    animator.ResetTrigger(TurnLeft);
                    
                    animator.SetTrigger(sign > 0f ? TurnRight : TurnLeft);
                }
                
                isTurning = true;
            }

            transform.rotation *= Quaternion.Euler(0f, turnInput, 0f);
            
            float lastProgress = turnCurve.Evaluate(turnProgress);
            turnProgress += Time.deltaTime * turnSpeed;
            turnProgress = Mathf.Min(turnProgress, 1f);
            
            float deltaProgress = turnCurve.Evaluate(turnProgress) - lastProgress;

            _playerInput.x -= sign * turnInPlaceAngle * deltaProgress;
            
            transform.rotation *= Quaternion.Slerp(Quaternion.identity,
                Quaternion.Euler(0f, sign * turnInPlaceAngle, 0f), deltaProgress);
            
            if (Mathf.Approximately(turnProgress, 1f) && isTurning)
            {
                isTurning = false;
            }
        }

        protected float _jumpState = 0f;

        protected virtual void UpdateLookInput()
        {
            _freeLook = Input.GetKey(KeyCode.X);

            float deltaMouseX = Input.GetAxis("Mouse X") * sensitivity;
            float deltaMouseY = -Input.GetAxis("Mouse Y") * sensitivity;
            
            if (_freeLook)
            {
                // No input for both controller and animation component. We only want to rotate the camera

                _freeLookInput.x += deltaMouseX;
                _freeLookInput.y += deltaMouseY;

                _freeLookInput.x = Mathf.Clamp(_freeLookInput.x, -freeLookAngle.x, freeLookAngle.x);
                _freeLookInput.y = Mathf.Clamp(_freeLookInput.y, -freeLookAngle.y, freeLookAngle.y);

                return;
            }

            _freeLookInput = Vector2.Lerp(_freeLookInput, Vector2.zero, 
                FPSAnimLib.ExpDecayAlpha(15f, Time.deltaTime));
            
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
            transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, moveWeight);
            transform.rotation = Quaternion.Slerp(transform.rotation, moveRotation, _jumpState);
            _playerInput.x *= 1f - moveWeight;
            _playerInput.x *= 1f - _jumpState;

            charAnimData.SetAimInput(_playerInput);
            charAnimData.AddDeltaInput(new Vector2(deltaMouseX, charAnimData.deltaAimInput.y));
        }

        protected Quaternion lastRotation;
        protected Vector2 _cameraRecoilOffset;

        protected void UpdateRecoil()
        {
            if (Mathf.Approximately(_controllerRecoil.magnitude, 0f)
                && Mathf.Approximately(_cameraRecoilOffset.magnitude, 0f))
            {
                return;
            }
            
            float smoothing = 8f;
            float restoreSpeed = 8f;
            float cameraWeight = 0f;

            if (GetGun().recoilPattern != null)
            {
                smoothing = GetGun().recoilPattern.smoothing;
                restoreSpeed = GetGun().recoilPattern.cameraRestoreSpeed;
                cameraWeight = GetGun().recoilPattern.cameraWeight;
            }
            
            _controllerRecoil = Vector2.Lerp(_controllerRecoil, Vector2.zero,
                FPSAnimLib.ExpDecayAlpha(smoothing, Time.deltaTime));

            _playerInput += _controllerRecoil * Time.deltaTime;
            
            Vector2 clamp = Vector2.Lerp(Vector2.zero, new Vector2(90f, 90f), cameraWeight);
            _cameraRecoilOffset -= _controllerRecoil * Time.deltaTime;
            _cameraRecoilOffset = Vector2.ClampMagnitude(_cameraRecoilOffset, clamp.magnitude);

            if (_isFiring) return;

            _cameraRecoilOffset = Vector2.Lerp(_cameraRecoilOffset, Vector2.zero,
                FPSAnimLib.ExpDecayAlpha(restoreSpeed, Time.deltaTime));
        }

        protected virtual void Update()
        {
            Time.timeScale = timeScale;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit(0);
            }
            
            UpdateActionInput();
            UpdateLookInput();
            UpdateRecoil();

            charAnimData.moveInput = movementComponent.AnimatorVelocity;
            UpdateAnimController();
        }
        
        public void UpdateCameraRotation()
        {
            Vector2 input = _playerInput;
            input += _cameraRecoilOffset;
            
            (Quaternion, Vector3) cameraTransform =
                (transform.rotation * Quaternion.Euler(input.y, input.x, 0f),
                    firstPersonCamera.position);

            cameraHolder.rotation = cameraTransform.Item1;
            cameraHolder.position = cameraTransform.Item2;

            mainCamera.rotation = cameraHolder.rotation * Quaternion.Euler(_freeLookInput.y, _freeLookInput.x, 0f);
        }
    }
}