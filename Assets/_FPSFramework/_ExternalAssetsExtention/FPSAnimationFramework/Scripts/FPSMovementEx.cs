using _KMH_Framework;
using Demo.Scripts.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
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

        protected override void UpdatePoseState()
        {
            if (MovementState is FPSMovementState.Sprinting or FPSMovementState.InAir ||
                KeyInputManager.Instance == null)
            {
                return;
            }

            if (_keyData["Prone"].IsInput == true)
            {
                if (CanProne() == false)
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

            if (_keyData["Crouch"].IsInput == false)
            {
                return;
            }

            if (PoseState == FPSPoseState.Standing)
            {
                Crouch();

                _desiredGait = movementSettings.crouching;
                return;
            }

            if (CanUnCrouch() == false) 
            {
                return;
            }

            UnCrouch();
            _desiredGait = movementSettings.walking;
        }

        protected override void UpdateMovementState()
        {
            if ((MovementState == FPSMovementState.InAir &&
                IsInAir() == true) ||
                KeyInputManager.Instance == null)
            {
                return;
            }

            if (_keyData["Move Right"].IsInput == true)
            {
                _inputDirection.x = 1f;
            }
            else if (_keyData["Move Left"].IsInput == true)
            {
                _inputDirection.x = -1f;
            }
            else
            {
                _inputDirection.x = 0f;
            }

            if (_keyData["Move Forward"].IsInput == true)
            {
                _inputDirection.y = 1f;
            }
            else if (_keyData["Move Backward"].IsInput == true)
            {
                _inputDirection.y = -1f;
            }
            else
            {
                _inputDirection.y = 0f;
            }

            if (MovementState == FPSMovementState.Sliding &&
                Mathf.Approximately(_slideProgress, 1f) == false)
            {
                // Consume input, but do not allow cancelling sliding.
                return;
            }

            // Jump action overrides any other input
            if (TryJump() == true)
            {
                return;
            }

            if (TrySprint() == true)
            {
                return;
            }

            if (IsMoving() == false)
            {
                MovementState = FPSMovementState.Idle;
                return;
            }

            MovementState = FPSMovementState.Walking;
        }

        protected override bool TryJump()
        {
            if (_keyData["Jump"].IsInput == false ||
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
                _keyData["Sprint"].IsInput == false)
            {
                return false;
            }

            // Can Remap Other Keycode
            if (Input.GetKey(KeyCode.X) == true &&
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