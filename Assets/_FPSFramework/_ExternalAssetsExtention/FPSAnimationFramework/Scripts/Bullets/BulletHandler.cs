using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(Rigidbody))]
    public class BulletHandler : MonoBehaviour
    {
        [Header("=== BulletHandler ===")]
        [SerializeField]
        protected ProjectileType projectileType;

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
                    this.gameObject.ReturnPool(projectileType);
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

        protected virtual void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();
        }

        protected virtual void OnEnable()
        {
            CurrentPenetratePower = maxPenetratePower;
            if (projectileType == ProjectileType._577_450_SR_Bullet)
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

            this.gameObject.ReturnPool(projectileType);
        }

        protected virtual async UniTaskVoid PoolSmokeParticleAsync()
        {
            FxType smokeType = FxType.MuzzleFlashSmoke;
            GameObject pooledSmoke = smokeType.EnablePool(obj => obj.transform.SetPositionAndRotation(this.transform.position, this.transform.rotation));

            await UniTask.WaitForSeconds(10f);

            pooledSmoke.ReturnPool(smokeType);
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
                    ImpactType impactType = _impactable.ImpactType;
                    Vector3 impactPoint = hits[i].point;
                    Vector3 impactAngle = hits[i].normal + hits[i].point;

                    impactType.EnablePool(OnBeforeEnableAction);
                    void OnBeforeEnableAction(GameObject impactObj)
                    {
                        impactObj.transform.position = impactPoint;
                        impactObj.transform.LookAt(impactAngle);
                    }

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
                    
                    CurrentPenetratePower -= _impactable.Thickness;
                }
            }
        }
    }
}