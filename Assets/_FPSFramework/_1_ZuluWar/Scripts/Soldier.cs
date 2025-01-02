using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    [RequireComponent(typeof(AudioSource))]
    public class Soldier : MonoBehaviour
    {
        protected AudioSource audioSource;

        [SerializeField]
        protected Animator animator;
        [SerializeField]
        protected KeyframeHandler keyframeHandler;

        [Space(10)]
        [SerializeField]
        protected Transform fireT;
        [SerializeField]
        protected ParticleSystem muzzleFlash;
        [SerializeField]
        protected AudioClip[] fireSoundClips;

        [Space(10)]
        [SerializeField]
        protected float verticalAngle = 90f;
        [SerializeField]
        protected float attackRange = 100f;
        [SerializeField]
        protected ProjectileType projectileType;

        [Space(10)]
        [SerializeField]
        protected float attackInterval = 5f;
        [SerializeField]
        protected float attackIntervalRandomRange = 1f;

        protected Quaternion initialRotation;
        protected Vector3 initialDirection;

        protected WarriorController targetController = null;

        protected void Awake()
        {
            initialRotation = this.transform.rotation;
            initialDirection = this.transform.forward;

            audioSource = this.GetComponent<AudioSource>();
            keyframeHandler.OnKeyframeReached += OnKeyframeReached;

            FindUnitAsync().Forget();
        }

        protected void Update()
        {
            if (targetController != null)
            {
                Vector3 direction = (targetController.MiddlePos - this.transform.position).normalized;
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 5f);
            }
            else
            {
                this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, initialRotation, 5f);
            }
        }

        protected async UniTaskVoid FindUnitAsync()
        {
            while (true)
            {
                if (targetController == null)
                {
                    Collider[] overlappedColliders = Physics.OverlapSphere(this.transform.position, attackRange);
                    foreach (Collider overlappedCollider in overlappedColliders)
                    {
                        if (overlappedCollider.TryGetComponent<WarriorController>(out WarriorController warriorController) == true &&
                            warriorController._State != State.Dead)
                        {
                            Vector3 diffVector = warriorController.transform.position - this.transform.position;
                            Vector3 directionToTarget = diffVector.normalized;
                            float distanceToTarget = diffVector.magnitude;

                            float angleToTarget = Vector3.Angle(initialDirection, directionToTarget);

                            if (angleToTarget < verticalAngle * 0.5f && distanceToTarget <= attackRange)
                            {
                                targetController = warriorController;
                                await AttackAsync();
                            }
                            else
                            {
                                targetController = null;
                            }
                        }
                    }
                }

                float randomizedInterval = Random.Range(0.5f, 1f);
                await UniTask.WaitForSeconds(randomizedInterval, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }

        protected async UniTask AttackAsync()
        {
            while (targetController._State != State.Dead)
            {
                animator.SetTrigger("shot");

                float randomizedInterval = Random.Range(attackInterval - attackIntervalRandomRange, attackInterval + attackIntervalRandomRange);
                await UniTask.WaitForSeconds(randomizedInterval, cancellationToken: this.GetCancellationTokenOnDestroy());
            }

            targetController = null;
        }

        protected void OnKeyframeReached(int index)
        {
            if (index == 0)
            {
                FireBullet();
            }
        }

        protected void FireBullet()
        {
            // Pool Bullet
            projectileType.EnablePool<BulletHandler>(OnBeforePool);
            void OnBeforePool(BulletHandler bullet)
            {
                bullet.transform.position = fireT.position;
                float distance = (targetController.MiddlePos - fireT.position).magnitude;

                PredictDataBundle predictBundle = PredictDataBundle.GetPredictData(projectileType);
                Vector3 predictPos = predictBundle.GetPredictedPosition(fireT.position, targetController.MiddlePos, targetController.Velocity);
                Vector3 direction = (predictPos - fireT.position).normalized;
                bullet.transform.eulerAngles = Quaternion.LookRotation(direction).eulerAngles;
            }

            // Release Fx
            int randomized = Random.Range(0, fireSoundClips.Length);
            audioSource.PlayOneShot(fireSoundClips[randomized]);
            muzzleFlash.Play();
        }

        private void OnDrawGizmos()
        {
            if (targetController == null)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawWireSphere(this.transform.position, attackRange);
        }
    }
}
