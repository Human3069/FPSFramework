using Demo.Scripts.Runtime;

using System.Collections.Generic;
using UnityEngine;

namespace FPSFramework
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class WeaponEx : Weapon, IWeapon
    {
        private const string LOG_FORMAT = "<color=white><b>[WeaponEx]</b></color> {0}";

        protected Collider _collider;
        protected Rigidbody _rigidbody;

        public virtual void Initialize()
        {
            Debug.LogFormat(LOG_FORMAT, "Initialize()");

            _collider = this.GetComponent<Collider>();
            _rigidbody = this.GetComponent<Rigidbody>();

            _collider.enabled = false;
            _rigidbody.isKinematic = true;
        }

        public virtual void DoInteract()
        {
            Debug.LogFormat(LOG_FORMAT, "DoInteract()");

            this.gameObject.SetActive(false);
        }

        public virtual void Release(Vector3 worldPos)
        {
            Debug.LogFormat(LOG_FORMAT, "Release(), worldPos : " + worldPos);

            this.transform.position = worldPos;

            _collider.enabled = true;
            _rigidbody.isKinematic = false;
        }
    }
}