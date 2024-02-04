using _KMH_Framework;
using Demo.Scripts.Runtime;
using System.Collections;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class WeaponEx : Weapon, IWeapon
    {
        private const string MUZZLE_FLASH_NAME = "MuzzleFlash";
        private const string FIRE_POINT_NAME = "FirePoint";

        [Header("WeaponEx")]
        [SerializeField]
        protected BulletHandler.BulletType _bulletType;
        [SerializeField]
        protected AttatchmentHandler attatchmentHandler;
        public AttatchmentHandler AttatchmentHandler
        {
            get
            {
                return attatchmentHandler;
            }
        }

        protected MagnifableSight magnifable = null;

        [Space(10)]
        [SerializeField]
        protected int _maxMagCount;
        public int MaxMagCount
        {
            get
            {
                return _maxMagCount;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected int _currentMagCount;
        public int CurrentMagCount
        {
            get
            {
                return _currentMagCount;
            }
            set
            {
                _currentMagCount = value;
            }
        }

        protected string bulletName;

        protected Collider _collider;
        protected Rigidbody _rigidbody;
        protected VisualFxPlayer _muzzleFlashFx;
        protected Transform _firePoint;

        protected bool _isAiming = false;
        public bool IsAiming
        {
            get
            {
                return _isAiming;
            }
            set
            {
                if (_isAiming != value)
                {
                    _isAiming = value;

                    if (value == true && magnifable != null)
                    {
                        StartCoroutine(PostAimingOnIfMagnifable());
                    }
                    else
                    {
                        FPSManager.Instance.AimThreshold = 1f;
                    }
                }
            }
        }

        protected IEnumerator PostAimingOnIfMagnifable()
        {
            while (IsAiming == true)
            {
                FPSManager.Instance.AimThreshold = 1f / magnifable.MagnifyRatio;

                yield return null;
            }
        }

        protected virtual void Awake()
        {
            if (attatchmentHandler.SelectedSight is MagnifableSight)
            {
                magnifable = attatchmentHandler.SelectedSight as MagnifableSight;
            }

            StartCoroutine(PostAwake());
        }

        protected IEnumerator PostAwake()
        {
            while (BulletPoolManager.Instance == null ||
                   BulletPoolManager.Instance.IsReady == false)
            {
                yield return null;
            }

            bulletName = BulletHandler.GetName(_bulletType);
        }

        public override Transform GetAimPoint()
        {
            return attatchmentHandler.SelectedSight.AimPoint;
        }

        public virtual void Initialize()
        {
            Debug.LogFormat(_Log._Format(this), "Initialize()");

            _collider = this.GetComponent<Collider>();
            _rigidbody = this.GetComponent<Rigidbody>();
            _muzzleFlashFx = this.transform.Find(MUZZLE_FLASH_NAME).GetComponent<VisualFxPlayer>();
            _firePoint = this.transform.Find(FIRE_POINT_NAME);

            attatchmentHandler.SelectedSight.IsEquipped = true;
       
            _collider.enabled = false;
            _rigidbody.isKinematic = true;
        }

        public virtual void DoInteract() // Weapon Equip
        {
            Debug.LogFormat(_Log._Format(this), "DoInteract()");

            this.gameObject.SetActive(false);
        }

        public virtual void Release(Vector3 worldPos, Quaternion worldRot) // Weapon Release
        {
            Debug.LogFormat(_Log._Format(this), "Release(), worldPos : " + worldPos + ", worldRot : " + worldRot);

            this.transform.position = worldPos;
            this.transform.rotation = worldRot;

            attatchmentHandler.SelectedSight.IsEquipped = false;

            _collider.enabled = true;
            _rigidbody.isKinematic = false;
        }

        public override void OnFire()
        {
            Debug.LogFormat(_Log._Format(this), "_firePoint.pos : " + _firePoint.position + ", _firePoint.name : " + _firePoint.name);

            BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].EnableObject((weaponTransformData.pivotPoint.position + weaponTransformData.pivotPoint.forward), weaponTransformData.pivotPoint.rotation);
            _muzzleFlashFx.Play();

            base.OnFire();
        }
    }
}