using _KMH_Framework;
using Demo.Scripts.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace FPSFramework
{
    [RequireComponent(typeof(FPSControllerEx))]
    public class FPSMovementEx : FPSMovement
    {
        protected FPSControllerEx fpsController;

        protected Dictionary<string, KeySetting> _keyData
        {
            get
            {
                return KeyInputManager.Instance.KeyData;
            }
        }

        protected void Awake()
        {
            fpsController = this.GetComponent<FPSControllerEx>();
        }

        protected override void UpdateMovementState()
        {
            if ((MovementState == FPSMovementState.InAir &&
                IsInAir() == true) ||
                KeyInputManager.Instance == null ||
                KeyInputManager.Instance.IsInitialized == false)
            {
                return;
            }

            if (_keyData[KeyInputManager.KEY_MOVE_RIGHT].isInput == true)
            {
                _inputDirection.x = 1f;
            }
            else if (_keyData[KeyInputManager.KEY_MOVE_LEFT].isInput == true)
            {
                _inputDirection.x = -1f;
            }
            else
            {
                _inputDirection.x = 0f;
            }

            if (_keyData[KeyInputManager.KEY_MOVE_FORWARD].isInput == true)
            {
                _inputDirection.y = 1f;
            }
            else if (_keyData[KeyInputManager.KEY_MOVE_BACKWARD].isInput == true)
            {
                _inputDirection.y = -1f;
            }
            else
            {
                _inputDirection.y = 0f;
            }

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

        protected override bool TryJump()
        {
            if (_keyData[KeyInputManager.KEY_JUMP].isInput == false ||
                PoseState == FPSPoseState.Crouching)
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

        protected override bool TrySprint()
        {
            if (PoseState is FPSPoseState.Crouching or FPSPoseState.Prone)
            {
                return false;
            }

            if (_inputDirection.y <= 0f ||
                _inputDirection.x != 0f ||
                _keyData[KeyInputManager.KEY_SPRINT].isInput == false)
            {
                return false;
            }

            if (Input.GetKey(movementSettings.slideKey) &&
                GetSpeedRatio() > 0.5f)
            {
                if (CanSlide() == false)
                {
                    return false;
                }

                MovementState = FPSMovementState.Sliding;
                return true;
            }

            if (CanSprint() == false)
            {
                return false;
            }

            if (fpsController.IsAiming() == true)
            {
                fpsController.DisableAim();
            }

            MovementState = FPSMovementState.Sprinting;
            return true;
        }
    }
}