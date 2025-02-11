// Designed by Kinemation, 2023

using FPS_Framework;
using Kinemation.FPSFramework.Runtime.Camera;
using Kinemation.FPSFramework.Runtime.Core.Types;
using Kinemation.FPSFramework.Runtime.FPSAnimator;

using System.Collections.Generic;
using UnityEngine;

namespace Demo.Scripts.Runtime
{
    public enum OverlayType
    {
        Default,
        Pistol,
        Rifle
    }
    
    public class Weapon : FPSAnimWeapon
    {
        [Header("Animations")]
        public AnimSequence reloadClip;
        public AnimSequence grenadeClip;
        public AnimSequence fireClip;
        public OverlayType overlayType;
        
        [Header("Aiming")]
        public bool canAim = true;
        [SerializeField] protected List<Transform> scopes;
        
        [Header("Recoil")]
        public RecoilPattern recoilPattern;
        public FPSCameraShake cameraShake;
        
        protected Animator _animator;
        private int _scopeIndex;

        protected void Start()
        {
            _animator = GetComponentInChildren<Animator>();
        }

        public override Transform GetAimPoint()
        {
            _scopeIndex++;
            _scopeIndex = _scopeIndex > scopes.Count - 1 ? 0 : _scopeIndex;
            return scopes[_scopeIndex];
        }
        
        public virtual void OnFire()
        {
            if (FPSControllerEx.Instance.CurrentEquipedWeapon == this)
            {
                if (_animator == null)
                {
                    return;
                }

                _animator.Play("Fire", 0, 0f);
            }
        }

        public virtual void Reload()
        {
            if (FPSControllerEx.Instance.CurrentEquipedWeapon == this)
            {
                if (_animator == null)
                {
                    return;
                }

                _animator.Rebind();
                _animator.Play("Reload", 0);
            }
        }
    }
}