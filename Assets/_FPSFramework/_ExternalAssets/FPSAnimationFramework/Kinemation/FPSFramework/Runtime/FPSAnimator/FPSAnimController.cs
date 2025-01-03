// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.Core.Components;
using Kinemation.FPSFramework.Runtime.Core.Playables;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.Layers;
using Kinemation.FPSFramework.Runtime.Recoil;

using UnityEngine;
using UnityEngine.Events;

namespace Kinemation.FPSFramework.Runtime.FPSAnimator
{
    // Animation Controller Interface
    // Make sure to derive your controller from this class
    public abstract class FPSAnimController : MonoBehaviour
    {
        protected CoreAnimComponent fpsAnimator;
        protected FPSCamera fpsCamera;
        protected RecoilAnimation recoilComponent;
        protected CharAnimData charAnimData;

        protected LookLayer internalLookLayer;
        protected AdsLayer internalAdsLayer;
        
        // Used primarily for function calls from Animation Events
        // Runs once at the beginning of the next update
        protected UnityEvent queuedAnimEvents;
        
        protected CoreAnimGraph GetAnimGraph()
        {
            return fpsAnimator.animGraph;
        }

        // Call this once when the character is initialized
        protected void InitAnimController()
        {
            fpsAnimator = GetComponentInChildren<CoreAnimComponent>();
            fpsAnimator.animGraph.InitPlayableGraph();
            fpsAnimator.InitializeComponent();

            recoilComponent = GetComponentInChildren<RecoilAnimation>();
            charAnimData = new CharAnimData();

            fpsCamera = GetComponentInChildren<FPSCamera>();
            internalLookLayer = GetComponentInChildren<LookLayer>();
            internalAdsLayer = GetComponentInChildren<AdsLayer>();

            if (fpsCamera != null)
            {
                fpsCamera.rootBone = fpsAnimator.ikRigData.rootBone;
            }
        }

        // Call this once when the character is initialized
        protected void InitAnimController(UnityAction cameraDelegate)
        {
            InitAnimController();
            fpsAnimator.onPostUpdate.AddListener(cameraDelegate);

            if (fpsCamera == null)
            {
                return;
            }
            fpsAnimator.onPostUpdate.AddListener(fpsCamera.UpdateCamera);
        }
        
        // Call this when equipping a new weapon
        protected virtual void InitWeapon(FPSAnimWeapon weapon)
        {
            recoilComponent.Init(weapon.weaponAsset.recoilData, weapon.fireRate, weapon.CurrentFireMode);
            fpsAnimator.OnGunEquipped(weapon.weaponAsset, weapon.weaponTransformData);
            
            fpsAnimator.ikRigData.weaponTransform = weapon.weaponAsset.weaponBone;

            if (internalLookLayer != null)
            {
                internalLookLayer.SetAimOffsetTable(weapon.weaponAsset.aimOffsetTable);
            }
            
            var pose = weapon.weaponAsset.overlayPose;

            if (pose == null)
            {
                Debug.LogError("FPSAnimController: OverlayPose is null! Make sure to assign it in the weapon prefab.");
                return;
            }

            fpsAnimator.OnPrePoseSampled();
            PlayPose(weapon.weaponAsset.overlayPose);
            fpsAnimator.OnPoseSampled();
            
            if (fpsCamera != null)
            {
                fpsCamera.cameraData = weapon.weaponAsset.adsData.cameraData;
            }
        }

        // Call this when changing sights
        protected virtual void InitAimPoint(FPSAnimWeapon weapon)
        {
            fpsAnimator.OnSightChanged(weapon.GetAimPoint());
            
            if (internalAdsLayer != null)
            {
                internalAdsLayer.UpdateAimPoint();
            }
        }

        // Call this during Update after all the gameplay logic
        protected void UpdateAnimController()
        {
            if (queuedAnimEvents != null)
            {
                queuedAnimEvents.Invoke();
                queuedAnimEvents = null;
            }

            if (recoilComponent != null)
            {
                charAnimData.recoilAnim = 
                    new LocRot(recoilComponent.OutLoc, Quaternion.Euler(recoilComponent.OutRot));
            }
            
            fpsAnimator.SetCharData(charAnimData);
            fpsAnimator.ikRigData.RetargetWeaponBone();
            GetAnimGraph().UpdateGraph();
            fpsAnimator.ScheduleJobs();

            if (fpsCamera != null)
            {
                float pitch = GetAnimGraph().GetCurveValue(CurveLib.Curve_Camera_Pitch);
                float yaw = GetAnimGraph().GetCurveValue(CurveLib.Curve_Camera_Yaw);
                float roll = GetAnimGraph().GetCurveValue(CurveLib.Curve_Camera_Roll);

                Quaternion rot = Quaternion.Euler(pitch, yaw, roll);
                fpsCamera.UpdateCameraAnimation(rot.normalized);
            }
        }

        protected void OnInputAim(bool isAiming)
        {
            if (fpsCamera != null)
            {
                fpsCamera.IsAiming = isAiming;
            }
        }

        // Call this to play a Camera shake
        protected void PlayCameraShake(FPSCameraShake shake)
        {
            if (fpsCamera != null && shake != null)
            {
                fpsCamera.PlayShake(shake.shakeInfo);
            }
        }
        
        // Call this to play a static pose on the character upper body
        protected void PlayPose(AnimSequence motion)
        {
            if (motion == null) return;
            fpsAnimator.animGraph.PlayPose(motion);
        }

        // Call this to play an animation on the character upper body
        protected void PlayAnimation(AnimSequence motion, float startTime = 0f)
        {
            if (motion == null) return;
            fpsAnimator.animGraph.PlayAnimation(motion, startTime);
        }
        
        protected void StopAnimation(float blendTime = 0f)
        {
            fpsAnimator.animGraph.StopAnimation(blendTime);
        }
    }
}