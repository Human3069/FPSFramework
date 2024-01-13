// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.FPSAnimator;
using UnityEngine;
using UnityEngine.Events;

namespace Demo.Scripts.Runtime
{
    public enum FPSMovementState
    {
        Idle,
        Walking,
        Sprinting,
        InAir,
        Sliding
    }

    public enum FPSPoseState
    {
        Standing,
        Crouching,
        Prone
    }
    
    public class FPSMovement : MonoBehaviour
    {
        public delegate bool ConditionDelegate();
        
        [SerializeField] protected FPSMovementSettings movementSettings;
        [SerializeField] public Transform rootBone;
        
        [SerializeField] public UnityEvent onStartMoving;
        [SerializeField] public UnityEvent onStopMoving;
        
        [SerializeField] public UnityEvent onSprintStarted;
        [SerializeField] public UnityEvent onSprintEnded;

        [SerializeField] public UnityEvent onCrouch;
        [SerializeField] public UnityEvent onUncrouch;
        
        [SerializeField] public UnityEvent onProneStarted;
        [SerializeField] public UnityEvent onProneEnded;

        [SerializeField] public UnityEvent onJump;
        [SerializeField] public UnityEvent onLanded;

        [SerializeField] public UnityEvent onSlideStarted;
        [SerializeField] public UnityEvent onSlideEnded;

        public ConditionDelegate slideCondition;
        public ConditionDelegate proneCondition;
        public ConditionDelegate sprintCondition;
        
        public FPSMovementState MovementState { get; protected set; }
        public FPSPoseState PoseState { get; protected set; }

        public Vector2 AnimatorVelocity { get; protected set; }
        
        protected CharacterController _controller;
        protected Animator _animator;
        protected Vector2 _inputDirection;

        public Vector3 MoveVector { get; protected set; }
        
        protected Vector3 _velocity;

        protected float _originalHeight;
        protected Vector3 _originalCenter;
        
        protected GaitSettings _desiredGait;
        protected float _slideProgress = 0f;

        protected Vector3 _prevPosition;
        protected Vector3 _velocityVector;

        protected static readonly int InAir = Animator.StringToHash("InAir");
        protected static readonly int MoveX = Animator.StringToHash("MoveX");
        protected static readonly int MoveY = Animator.StringToHash("MoveY");
        protected static readonly int Velocity = Animator.StringToHash("Velocity");
        protected static readonly int Moving = Animator.StringToHash("Moving");
        protected static readonly int Crouching = Animator.StringToHash("Crouching");
        protected static readonly int Sliding = Animator.StringToHash("Sliding");
        protected static readonly int Sprinting = Animator.StringToHash("Sprinting");
        protected static readonly int Proning = Animator.StringToHash("Proning");

        protected float _sprintAnimatorInterp = 8f;

        protected bool _wasMoving = false;

        public bool IsInAir()
        {
            return !_controller.isGrounded;
        }
        
        protected bool IsMoving()
        {
            return !Mathf.Approximately(_inputDirection.normalized.magnitude, 0f);
        }

        protected float GetSpeedRatio()
        {
            return _velocity.magnitude / _desiredGait.velocity;
        }

        protected bool CanSlide()
        {
            return slideCondition == null || slideCondition.Invoke();
        }

        protected bool CanSprint()
        {
            return sprintCondition == null || sprintCondition.Invoke();
        }

        protected bool CanProne()
        {
            return proneCondition == null || proneCondition.Invoke(); 
        }

        protected virtual bool TryJump()
        {
            if (!Input.GetKeyDown(movementSettings.jumpKey) || PoseState == FPSPoseState.Crouching)
            {
                return false;
            }

            if (PoseState == FPSPoseState.Prone)
            {
                CancelProne();
                return false;
            }
            
            MovementState = FPSMovementState.InAir;
            return true;
        }

        protected virtual bool TrySprint()
        {
            if (PoseState is FPSPoseState.Crouching or FPSPoseState.Prone)
            {
                return false;
            }

            if (_inputDirection.y <= 0f || _inputDirection.x != 0f || !Input.GetKey(movementSettings.sprintKey))
            {
                return false;
            }

            if (Input.GetKey(movementSettings.slideKey) && GetSpeedRatio() > 0.5f)
            {
                if (!CanSlide()) return false;
                
                MovementState = FPSMovementState.Sliding;
                return true;
            }
            
            if (!CanSprint()) return false;
            
            MovementState = FPSMovementState.Sprinting;
            return true;
        }

        protected bool CanUnCrouch()
        {
            float height = _originalHeight - _controller.radius * 2f;
            Vector3 position = rootBone.TransformPoint(_originalCenter + Vector3.up * height / 2f);
            return !Physics.CheckSphere(position, _controller.radius);
        }

        protected void EnableProne()
        {
            Crouch();
            PoseState = FPSPoseState.Prone;
            _animator.SetBool(Crouching, false);
            _animator.SetBool(Proning, true);
            
            onProneStarted?.Invoke();
            _desiredGait = movementSettings.prone;
        }

        protected void CancelProne()
        {
            if (!CanUnCrouch()) return;
            UnCrouch();
            PoseState = FPSPoseState.Standing;
            _animator.SetBool(Proning, false);
            
            onProneEnded?.Invoke();
            _desiredGait = movementSettings.walking;
        }

        protected void Crouch()
        {
            float crouchedHeight = _originalHeight * movementSettings.crouchRatio;
            float heightDifference = _originalHeight - crouchedHeight;

            _controller.height = crouchedHeight;

            // Adjust the center position so the bottom of the capsule remains at the same position
            Vector3 crouchedCenter = _originalCenter;
            crouchedCenter.y -= heightDifference / 2;
            _controller.center = crouchedCenter;

            PoseState = FPSPoseState.Crouching;
            
            _animator.SetBool(Crouching, true);
            onCrouch.Invoke();
        }

        protected void UnCrouch()
        {
            _controller.height = _originalHeight;
            _controller.center = _originalCenter;
            
            PoseState = FPSPoseState.Standing;
            
            _animator.SetBool(Crouching, false);
            onUncrouch.Invoke();
        }

        protected void UpdatePoseState()
        {
            if (MovementState is FPSMovementState.Sprinting or FPSMovementState.InAir)
            {
                return;
            }

            if (Input.GetKeyDown(movementSettings.proneKey))
            {
                if (!CanProne())
                {
                    return;
                }
                
                if (PoseState == FPSPoseState.Prone)
                {
                    CancelProne();
                }
                else
                {
                    EnableProne();
                }
                
                return;
            }

            if (!Input.GetKeyDown(movementSettings.crouchKey))
            {
                return;
            }

            if (PoseState == FPSPoseState.Standing)
            {
                Crouch();

                _desiredGait = movementSettings.crouching;
                return;
            }

            if (!CanUnCrouch()) return;

            UnCrouch();
            _desiredGait = movementSettings.walking;
        }

        protected virtual void UpdateMovementState()
        {
            if (MovementState == FPSMovementState.InAir && IsInAir())
            {
                // Do not update player movement while jumping or falling
                return;
            }
            
            // Get the current player input
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            _inputDirection.x = moveX;
            _inputDirection.y = moveY;

            if (MovementState == FPSMovementState.Sliding && !Mathf.Approximately(_slideProgress, 1f))
            {
                // Consume input, but do not allow cancelling sliding.
                return;
            }

            // Jump action overrides any other input
            if (TryJump())
            {
                return;
            }
            
            if (TrySprint())
            {
                return;
            }

            if (!IsMoving())
            {
                MovementState = FPSMovementState.Idle;
                return;
            }
            
            MovementState = FPSMovementState.Walking;
        }

        protected void OnMovementStateChanged(FPSMovementState prevState)
        {
            if (prevState == FPSMovementState.InAir)
            {
                onLanded.Invoke();
            }

            if (prevState == FPSMovementState.Sprinting)
            {
                _sprintAnimatorInterp = 7f;
                onSprintEnded.Invoke();
            }

            if (prevState == FPSMovementState.Sliding)
            {
                _sprintAnimatorInterp = 15f;
                onSlideEnded.Invoke();

                if (CanUnCrouch())
                {
                    UnCrouch();
                }
            }
            
            if (MovementState == FPSMovementState.Idle)
            {
                float prevVelocity = _desiredGait.velocity;
                _desiredGait = movementSettings.idle;
                _desiredGait.velocity = prevVelocity;
                return;
            }

            if (MovementState == FPSMovementState.InAir)
            {
                _velocity.y = movementSettings.jumpHeight;
                onJump.Invoke();
                return;
            }

            if (MovementState == FPSMovementState.Sprinting)
            {
                _desiredGait = movementSettings.sprinting;
                onSprintStarted.Invoke();
                return;
            }

            if (MovementState == FPSMovementState.Sliding)
            {
                _desiredGait.velocitySmoothing = movementSettings.slideDirectionSmoothing;
                _slideProgress = 0f;
                onSlideStarted.Invoke();
                Crouch();
                return;
            }

            if (PoseState == FPSPoseState.Crouching)
            {
                _desiredGait = movementSettings.crouching;
                return;
            }

            if (PoseState == FPSPoseState.Prone)
            {
                _desiredGait = movementSettings.prone;
                return;
            }
            
            // Walking state
            _desiredGait = movementSettings.walking;
        }

        protected void UpdateSliding()
        {
            // 1. Extract the slide animation.
            float slideAmount = movementSettings.slideCurve.Evaluate(_slideProgress);
            
            // 2. Apply sliding to both current and desired velocity vectors.
            // Here we just want to interpolate between the same velocities, but different directions.

            _velocity *= slideAmount;

            Vector3 desiredVelocity = _velocity;
            desiredVelocity.y = -movementSettings.gravity;
            MoveVector = desiredVelocity;
            
            _slideProgress = Mathf.Clamp01(_slideProgress + Time.deltaTime * movementSettings.slideSpeed);
        }
        
        protected void UpdateGrounded()
        {
            var normInput = _inputDirection.normalized;
            var desiredVelocity = rootBone.right * normInput.x + rootBone.forward * normInput.y;

            desiredVelocity *= _desiredGait.velocity;

            desiredVelocity = Vector3.Lerp(_velocity, desiredVelocity, 
                FPSAnimLib.ExpDecayAlpha(_desiredGait.velocitySmoothing, Time.deltaTime));
            
            _velocity = desiredVelocity;

            desiredVelocity.y = -movementSettings.gravity;
            MoveVector = desiredVelocity;
        }
        
        protected void UpdateInAir()
        {
            var normInput = _inputDirection.normalized;
            _velocity.y -= movementSettings.gravity * Time.deltaTime;
            _velocity.y = Mathf.Max(-movementSettings.maxFallVelocity, _velocity.y);
            
            var desiredVelocity = rootBone.right * normInput.x + rootBone.forward * normInput.y;
            desiredVelocity *= _desiredGait.velocity;

            desiredVelocity = Vector3.Lerp(_velocity, desiredVelocity * movementSettings.airFriction, 
                FPSAnimLib.ExpDecayAlpha(movementSettings.airVelocity, Time.deltaTime));

            desiredVelocity.y = _velocity.y;
            _velocity = desiredVelocity;
            
            MoveVector = desiredVelocity;
        }
        
        protected void UpdateMovement()
        {
            _controller.Move(MoveVector * Time.deltaTime);
        }

        protected void UpdateAnimatorParams()
        {
            var animatorVelocity = _inputDirection;
            animatorVelocity *= MovementState == FPSMovementState.InAir ? 0f : 1f;

            AnimatorVelocity = Vector2.Lerp(AnimatorVelocity, animatorVelocity, 
                FPSAnimLib.ExpDecayAlpha(_desiredGait.velocitySmoothing, Time.deltaTime));

            _animator.SetFloat(MoveX, AnimatorVelocity.x);
            _animator.SetFloat(MoveY, AnimatorVelocity.y);
            _animator.SetFloat(Velocity, AnimatorVelocity.magnitude);
            _animator.SetBool(InAir, IsInAir());
            _animator.SetBool(Moving, IsMoving());

            // Sprinting needs to be blended manually
            float a = _animator.GetFloat(Sprinting);
            float b = MovementState == FPSMovementState.Sprinting ? 1f : 0f;

            a = Mathf.Lerp(a, b, FPSAnimLib.ExpDecayAlpha(_sprintAnimatorInterp, Time.deltaTime));
            _animator.SetFloat(Sprinting, a);
        }

        protected void Start()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponentInChildren<Animator>();
            
            _originalHeight = _controller.height;
            _originalCenter = _controller.center;
            
            MovementState = FPSMovementState.Idle;
            PoseState = FPSPoseState.Standing;

            _desiredGait = movementSettings.walking;
        }
        
        protected void Update()
        {
            var prevState = MovementState;
            UpdateMovementState();
            UpdatePoseState();
            
            if (prevState != MovementState)
            {
                OnMovementStateChanged(prevState);
            }

            bool isMoving = IsMoving();
            
            if (_wasMoving != isMoving)
            {
                if (isMoving)
                {
                    onStartMoving?.Invoke();
                }
                else
                {
                    onStopMoving?.Invoke();
                }
            }
            
            _wasMoving = isMoving;

            if (MovementState == FPSMovementState.InAir)
            {
                UpdateInAir();
            }
            else if (MovementState == FPSMovementState.Sliding)
            {
                UpdateSliding();
            }
            else
            {
                UpdateGrounded();
            }

            UpdateMovement();
            UpdateAnimatorParams();
        }
    }
}