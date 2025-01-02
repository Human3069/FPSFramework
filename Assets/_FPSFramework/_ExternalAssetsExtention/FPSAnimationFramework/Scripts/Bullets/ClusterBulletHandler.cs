using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using UnityEngine;

namespace FPS_Framework
{
    public class ClusterBulletHandler : BulletHandler
    {
        [Header("=== ClusterBulletHandler ===")]
        [SerializeField]
        protected ProjectileType clusterProjectileType;

        [Space(10)]
        [SerializeField]
        protected bool isReactOnStatic = true;
        [SerializeField]
        protected int clusterCount = 10;
        [SerializeField]
        protected float clusterDistance = 10f;
        [SerializeField]
        protected float clusterRandomAngle = 30f;

        [Space(10)]
        [SerializeField]
        protected ImpactType clusterImpactType;

        protected override async UniTaskVoid CheckTrajectoryAsync()
        {
            while (this.gameObject.activeSelf == true)
            {
                Vector3 startPos = currentPos == Vector3.zero ? enablePos : currentPos;
                Vector3 endPos = this.transform.position;
                Vector3 direction = (endPos - startPos).normalized;

                if (direction != Vector3.zero)
                {
                    this.transform.forward = direction;
                }

                if (Physics.Raycast(this.transform.position, direction, out RaycastHit hit, clusterDistance) == true)
                {
                    if (isReactOnStatic == false)
                    {
                        OnCluster();
                    }
                    else if (hit.collider.gameObject.isStatic == true)
                    {
                        OnCluster();
                    }
                }

                currentPos = this.transform.position;

                await UniTask.NextFrame(this.GetCancellationTokenOnDestroy());
            }
        }

        protected virtual void OnCluster()
        {
            for (int i = 0; i < clusterCount; i++)
            {
                Vector3 randomCircle = Random.onUnitSphere;
                Vector3 randomDir = Vector3.Slerp(this.transform.forward, randomCircle, Random.Range(0f, clusterRandomAngle / 360f));

                clusterProjectileType.EnablePool<BulletHandler>(OnBeforeEnableAction);
                void OnBeforeEnableAction(BulletHandler clusteredBulletHnadler)
                {
                    clusteredBulletHnadler.transform.position = this.transform.position;
                    clusteredBulletHnadler.transform.forward = randomDir;
                }
            }

            clusterImpactType.EnablePool(x => x.transform.position = this.transform.position);
            this.gameObject.ReturnPool(projectileType);
        }
    }
}