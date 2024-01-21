using _KMH_Framework;
using Demo.Scripts.Runtime;

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FPSFramework
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class WeaponEx : Weapon, IWeapon
    {
        private const string LOG_FORMAT = "<color=white><b>[WeaponEx]</b></color> {0}";
        private const string MUZZLE_FLASH_NAME = "MuzzleFlash";
        private const string FIRE_POINT_NAME = "FirePoint";

        protected Collider _collider;
        protected Rigidbody _rigidbody;
        protected VisualFxPlayer _muzzleFlashFx;
        protected Transform _firePoint;

        public virtual void Initialize()
        {
            Debug.LogFormat(LOG_FORMAT, "Initialize()");

            _collider = this.GetComponent<Collider>();
            _rigidbody = this.GetComponent<Rigidbody>();
            _muzzleFlashFx = this.transform.Find(MUZZLE_FLASH_NAME).GetComponent<VisualFxPlayer>();
            _firePoint = this.transform.Find(FIRE_POINT_NAME);

            _collider.enabled = false;
            _rigidbody.isKinematic = true;
        }

        public virtual void DoInteract()
        {
            Debug.LogFormat(LOG_FORMAT, "DoInteract()");

            this.gameObject.SetActive(false);
        }

        public virtual void Release(Vector3 worldPos, Quaternion worldRot)
        {
            Debug.LogFormat(LOG_FORMAT, "Release(), worldPos : " + worldPos + ", worldRot : " + worldRot);

            this.transform.position = worldPos;
            this.transform.rotation = worldRot;

            _collider.enabled = true;
            _rigidbody.isKinematic = false;
        }

        public override void OnFire()
        {
            Debug.LogFormat(LOG_FORMAT, "_firePoint.pos : " + _firePoint.position + ", _firePoint.name : " + _firePoint.name);

            base.OnFire();

            _muzzleFlashFx.Play();
            BulletPoolManager.Instance.PoolHandlerDictionary[BulletPoolManager.COMMON_BULLET].EnableObject(_firePoint.position, _firePoint.rotation);
        }
    }
}