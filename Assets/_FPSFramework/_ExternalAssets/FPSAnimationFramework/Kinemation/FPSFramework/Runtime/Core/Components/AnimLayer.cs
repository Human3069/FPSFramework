// Designed by KINEMATION, 2023

using Kinemation.FPSFramework.Runtime.Attributes;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.FPSAnimator;
using System;
using UnityEngine;

namespace Kinemation.FPSFramework.Runtime.Core.Components
{
    [Serializable]
    public abstract class AnimLayer : MonoBehaviour
    {
        [Header("Layer Blending")] 
        [SerializeField, AnimCurveName(true)] protected string curveName;

        [SerializeField, Range(0f, 1f)] protected float layerAlpha = 1f;
        [SerializeField] protected float lerpSpeed;
        [Range(0f, 1f)] public float elbowsWeight = 0f;
        protected float smoothLayerAlpha;
        
        [Header("Misc")] 
        [SerializeField] protected bool drawDebugInfo = true;
        [SerializeField] public bool runInEditor;
        protected CoreAnimComponent core;

        public void SetLayerAlpha(float weight)
        {
            layerAlpha = Mathf.Clamp01(weight);
        }

        public float GetLayerAlpha()
        {
            return smoothLayerAlpha;
        }

        // Called in Start()
        public virtual void InitializeLayer()
        {
            core = GetComponent<CoreAnimComponent>();
        }

        public virtual bool CanUpdate()
        {
            return !Mathf.Approximately(smoothLayerAlpha, 0f) && !Mathf.Approximately(Time.deltaTime, 0f) 
                                                              && GetGunAsset() != null;
        }

        public virtual bool CanUseParallelExecution()
        {
            return false;
        }

        public virtual void ScheduleJobs()
        {
        }
        
        // Called before the main anim update loop.
        public virtual void PreUpdateLayer()
        {
            float target = !string.IsNullOrEmpty(curveName) ? GetAnimator().GetFloat(curveName) : layerAlpha;
            smoothLayerAlpha = CoreToolkitLib.InterpLayer(smoothLayerAlpha, Mathf.Clamp01(target), lerpSpeed, 
                Time.deltaTime);
        }

        // Called if we need to complete the job.
        public virtual void CompleteJobs()
        {
        }
        
        // Main anim update
        public virtual void UpdateLayer()
        {
        }
        
        // Called when an overlay pose is sampled
        public virtual void OnPoseSampled()
        {
        }
        
        protected WeaponAnimAsset GetGunAsset()
        {
            return core.weaponAsset;
        }

        protected CharAnimData GetCharData()
        {
            return core.characterData;
        }

        protected Transform GetMasterPivot()
        {
            return core.ikRigData.masterDynamic.obj.transform;
        }

        protected Transform GetRootBone()
        {
            return core.ikRigData.rootBone;
        }

        protected Transform GetPelvis()
        {
            return core.ikRigData.pelvis;
        }

        protected DynamicBone GetMasterIK()
        {
            return core.ikRigData.masterDynamic;
        }

        protected DynamicBone GetRightHandIK()
        {
            return core.ikRigData.rightHand;
        }

        protected DynamicBone GetLeftHandIK()
        {
            return core.ikRigData.leftHand;
        }
        
        protected Transform GetRightHand()
        {
            return core.ikRigData.rightHand.obj.transform;
        }

        protected Transform GetLeftHand()
        {
            return core.ikRigData.leftHand.obj.transform;
        }

        protected DynamicBone GetRightFootIK()
        {
            return core.ikRigData.rightFoot;
        }
        
        protected DynamicBone GetLeftFootIK()
        {
            return core.ikRigData.leftFoot;
        }

        protected Transform GetRightFoot()
        {
            return core.ikRigData.rightFoot.obj.transform;
        }

        protected Transform GetLeftFoot()
        {
            return core.ikRigData.leftFoot.obj.transform;
        }

        protected Animator GetAnimator()
        {
            return core.ikRigData.animator;
        }

        protected DynamicRigData GetRigData()
        {
            return core.ikRigData;
        }

        protected float GetCurveValue(string curve)
        {
            return core.animGraph.GetCurveValue(curve);
        }

        // Offsets master pivot only, without affecting the child IK bones
        // Useful if weapon has multiple pivots
        protected void OffsetMasterPivot(LocRot offset, float alpha = 1f)
        {
            LocRot rightHandTip = new LocRot(GetRightHand());
            LocRot leftHandTip = new LocRot(GetLeftHand());

            LocRot rightElbow = new LocRot(GetRightHandIK().hintTarget);
            LocRot leftElbow = new LocRot(GetLeftHandIK().hintTarget);

            GetMasterIK().Offset(GetMasterPivot(), offset.position, alpha);
            GetMasterIK().Offset(GetMasterPivot().rotation, offset.rotation, alpha);
            
            GetRightHandIK().Override(rightHandTip);
            GetLeftHandIK().Override(leftHandTip);

            GetRightHandIK().hintTarget.position = rightElbow.position;
            GetRightHandIK().hintTarget.rotation = rightElbow.rotation;
            
            GetLeftHandIK().hintTarget.position = leftElbow.position;
            GetLeftHandIK().hintTarget.rotation = leftElbow.rotation;
        }
        
        protected void OffsetMasterPivot(Vector3 offset, float alpha = 1f)
        {
            LocRot rightHandTip = new LocRot(GetRightHand());
            LocRot leftHandTip = new LocRot(GetLeftHand());

            LocRot rightElbow = new LocRot(GetRightHandIK().hintTarget);
            LocRot leftElbow = new LocRot(GetLeftHandIK().hintTarget);

            GetMasterIK().Offset(GetMasterPivot(), offset, alpha);

            GetRightHand().position = rightHandTip.position;
            GetRightHand().rotation = rightHandTip.rotation;

            GetLeftHand().position = leftHandTip.position;
            GetLeftHand().rotation = leftHandTip.rotation;

            GetRightHandIK().hintTarget.position = rightElbow.position;
            GetRightHandIK().hintTarget.rotation = rightElbow.rotation;
            
            GetLeftHandIK().hintTarget.position = leftElbow.position;
            GetLeftHandIK().hintTarget.rotation = leftElbow.rotation;
        }

        protected Transform GetPivotPoint()
        {
            return core.weaponTransformData.pivotPoint;
        }

        protected Transform GetAimPoint()
        {
            return core.weaponTransformData.aimPoint;
        }

        protected WeaponTransformData GetTransforms()
        {
            return core.weaponTransformData;
        }

        protected Vector3 GetPivotOffset()
        {
            return GetPivotPoint() != null ? GetPivotPoint().localPosition : Vector3.zero;
        }
    }
}