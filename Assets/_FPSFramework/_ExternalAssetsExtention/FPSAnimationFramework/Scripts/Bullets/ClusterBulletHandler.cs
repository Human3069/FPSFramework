using _KMH_Framework;
using Cysharp.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    public class ClusterBulletHandler : BulletHandler
    {
        [Header("=== ClusterBulletHandler ===")]
        [SerializeField]
        protected BulletType clusterBulletType;

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
        protected string impactName = "105mm_Explosion";

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

                await UniTask.NextFrame();
            }
        }

        protected virtual void OnCluster()
        {
            for (int i = 0; i < clusterCount; i++)
            {
                Vector3 randomized = Random.insideUnitCircle * clusterRandomAngle;
                Vector3 randomedEuler = this.transform.eulerAngles + randomized;
                BulletPoolManager.Instance.PoolHandlerDictionary[BulletHandler.GetName(clusterBulletType)].EnableObject(this.transform.position, Quaternion.Euler(randomedEuler));
            }

            ImpactPoolManager.Instance.PoolHandlerDictionary[impactName].EnableObject(this.transform.position, Quaternion.identity);
            BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].ReturnObject(this.gameObject);
        }
    }
}