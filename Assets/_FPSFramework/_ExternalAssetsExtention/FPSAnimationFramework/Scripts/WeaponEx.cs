using _KMH_Framework;
using Demo.Scripts.Runtime;
using System.Collections;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(AudioSource))]
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

                Invoke_OnAmmoValuesChanged();
            }
        }

        [Header("Sounds")]
        [SerializeField]
        protected AudioClip[] fireClips;

        protected string bulletName;

        protected Collider _collider;
        protected Rigidbody _rigidbody;
        protected AudioSource _audioSource;

        protected ParticleSystem _muzzleFlashFx;
        protected Transform _firePoint;

        public delegate void AmmoValuesChanged(int maxAmmo, int currentAmmo);
        public event AmmoValuesChanged OnAmmoValuesChanged;

        protected void Invoke_OnAmmoValuesChanged()
        {
            if (OnAmmoValuesChanged != null)
            {
                OnAmmoValuesChanged(MaxMagCount, CurrentMagCount);
            }
        }

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
            _collider = this.GetComponent<Collider>();
            _rigidbody = this.GetComponent<Rigidbody>();
            _audioSource = this.GetComponent<AudioSource>();
            _muzzleFlashFx = this.transform.Find(MUZZLE_FLASH_NAME).GetComponent<ParticleSystem>();
            _firePoint = this.transform.Find(FIRE_POINT_NAME);

            attatchmentHandler.SelectedSight.IsEquipped = true;
       
            _collider.enabled = false;
            _rigidbody.isKinematic = true;
        }

        public virtual void DoInteract() // Weapon Equip
        {
            this.gameObject.SetActive(false);

            CurrentMagCount = MaxMagCount;
        }

        public virtual void Release(Vector3 worldPos, Quaternion worldRot) // Weapon Release
        {
            this.transform.position = worldPos;
            this.transform.rotation = worldRot;

            attatchmentHandler.SelectedSight.IsEquipped = false;

            _collider.enabled = true;
            _rigidbody.isKinematic = false;
        }

        public override void Reload()
        {
            base.Reload();

            CurrentMagCount = MaxMagCount;
        }

        public override void OnFire()
        {
            CurrentMagCount--;
            BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].EnableObject((weaponTransformData.pivotPoint.position + weaponTransformData.pivotPoint.forward), weaponTransformData.pivotPoint.rotation);
            _muzzleFlashFx.Play();

            int randomIndex = Random.Range(0, fireClips.Length);
            _audioSource.PlayOneShot(fireClips[randomIndex]);

            base.OnFire();
        }
    }
}