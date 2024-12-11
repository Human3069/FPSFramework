using _KMH_Framework;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(Rigidbody))]
    public class BulletHandler : MonoBehaviour
    {
        public enum BulletType
        {
            _556_AR,
            _577_450_SR,
            _762_AR,
            _762_SR,
            _9_SMG,

            _Shrapnel_Piece,

            _57_Shrapnel,
            _40_Cannon,
        }

        [Header("=== BulletHandler ===")]
        [SerializeField]
        protected BulletType _bulletType;
        public BulletType _BulletType
        {
            get
            {
                return _bulletType;
            }
        }

        protected string bulletName;

        [Space(10)]
        [SerializeField]
        protected float lifeTime;
        [SerializeField]
        protected float speed;

        [Space(10)]
        [SerializeField]
        protected float maxPenetratePower;
        [ReadOnly]
        [SerializeField]
        protected float _currentPenetratePower;
        public float CurrentPenetratePower
        {
            get
            {
                return _currentPenetratePower;
            }
            set
            {
                _currentPenetratePower = Mathf.Clamp(value, 0f, maxPenetratePower);

                if (_currentPenetratePower == 0f)
                {
                    BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].ReturnObject(this.gameObject);
                }
            }
        }
        [SerializeField]
        protected float damage = 50f;

        protected Rigidbody _rigidbody;

        protected Vector3 enablePos;
        protected Vector3 disablePos;
        protected float flightDistance;

        protected Vector3 currentPos;

        public static string GetName(BulletType _type)
        {
            string bulletName;
            if (_type == BulletType._556_AR)
            {
                bulletName = BulletPoolManager._556_AR_BULLET;
            }
            else if (_type == BulletType._577_450_SR)
            {
                bulletName = BulletPoolManager._577_450_SR_BULLET;
            }
            else if (_type == BulletType._762_AR)
            {
                bulletName = BulletPoolManager._762_AR_BULLET;
            }
            else if (_type == BulletType._762_SR)
            {
                bulletName = BulletPoolManager._762_SR_BULLET;
            }
            else if (_type == BulletType._9_SMG)
            {
                bulletName = BulletPoolManager._9_SMG_BULLET;
            }

            else if (_type == BulletType._Shrapnel_Piece)
            {
                bulletName = BulletPoolManager._SHRAPNEL_PIECE;
            }

            else if (_type == BulletType._57_Shrapnel)
            {
                bulletName = BulletPoolManager._57_SHRAPNEL_BULLET;
            }
            else if (_type == BulletType._40_Cannon)
            {
                bulletName = BulletPoolManager._40_CANNON_BULLET;
            }
            else
            {
                bulletName = null;
                Debug.Assert(false);
            }

            return bulletName;
        }

        protected virtual void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();

            bulletName = GetName(_bulletType);
        }

        protected virtual void OnEnable()
        {
            CurrentPenetratePower = maxPenetratePower;
            if (_bulletType == BulletType._577_450_SR)
            {
                PoolSmokeParticleAsync().Forget();
            }

            _rigidbody.velocity = this.transform.forward * speed;
            _rigidbody.angularVelocity = Vector3.zero;

            currentPos = Vector3.zero;
            enablePos = this.transform.position;

            CheckLifetimeAsync().Forget();
            CheckTrajectoryAsync().Forget();
        }

        protected virtual async UniTaskVoid CheckLifetimeAsync()
        {
            await UniTask.WaitForSeconds(lifeTime);

            disablePos = this.transform.position;
            flightDistance = Vector3.Magnitude(enablePos - disablePos);

            BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].ReturnObject(this.gameObject);
        }

        protected virtual async UniTaskVoid PoolSmokeParticleAsync()
        {
            GameObject targetObj = BulletPoolManager.Instance.PoolHandlerDictionary["Particle_Smoke"].EnableObject(this.transform);

            await UniTask.WaitForSeconds(10f);

            BulletPoolManager.Instance.PoolHandlerDictionary["Particle_Smoke"].ReturnObject(targetObj);
        }

        protected virtual async UniTaskVoid CheckTrajectoryAsync()
        {
            while (this.gameObject.activeSelf == true)
            {
                Vector3 startPos = currentPos == Vector3.zero ? enablePos : currentPos;
                Vector3 endPos = this.transform.position;
                Vector3 direction = (endPos - startPos).normalized;
                float length = (endPos - startPos).magnitude;

                RaycastHit[] hits = Physics.RaycastAll(startPos, direction, length);
                if (hits != null && hits.Length != 0)
                {
                    OnHit(hits);
                }

                currentPos = this.transform.position;

                await UniTask.NextFrame();
            }
        }

        protected virtual void OnHit(RaycastHit[] hits)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.TryGetComponent<Impactable>(out Impactable _impactable) == true)
                {
                    Impactable.MaterialType _materialType = _impactable._MaterialType;
                    string _impactName = Impactable.GetName(_materialType);
                    Vector3 impactPoint = hits[i].point;
                    Vector3 impactAngle = hits[i].normal + hits[i].point;

                    GameObject particleObj = ImpactPoolManager.Instance.PoolHandlerDictionary[_impactName].EnableObject(impactPoint);
                    particleObj.transform.LookAt(impactAngle);

                    disablePos = this.transform.position;
                    flightDistance = Vector3.Magnitude(enablePos - disablePos);
                    if (_impactable.Warrior != null)
                    {
                        if (_impactable.Warrior.CurrentHealth > 0f)
                        {
                            FPSManager.Instance.PlayHitMarkerSoundIfAllowed();
                        }

                        _impactable.Warrior.CurrentHealth -= (damage * _impactable.DamageMultiplier);
                    }

                    if (_materialType == Impactable.MaterialType.ShootingTarget)
                    {
                        _impactable._ShootingTarget.ShowText(flightDistance.ToString("F1") + "m");
                    }

                    CurrentPenetratePower -= _impactable.Thickness;
                }
            }
        }
    }
}