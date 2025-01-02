using FPS_Framework.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    [ExecuteAlways]
    public class PredictableShooter : MonoBehaviour
    {
        [SerializeField]
        protected ProjectileType type;
        [SerializeField]
        protected Transform shootPoint;

        [Space(10)]
        [SerializeField]
        protected float maxDistance = 1000;

        [Space(10)]
        [SerializeField]
        protected List<PredictableData> predictableDataList = new List<PredictableData>();

        protected Vector3 straightPredictPoint;

        protected bool isFiring = false;
        protected float timer = 0f;

        protected void Awake()
        {
            if (Application.isPlaying == true)
            {
                gizmoPosList.Clear();
            }
        }

        protected void Update()
        {
            if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, maxDistance) == true)
            {
                straightPredictPoint = hit.point;
                Debug.DrawLine(shootPoint.position, hit.point, Color.red);
            }
            else
            {
                straightPredictPoint = shootPoint.position + shootPoint.forward * maxDistance;
                Debug.DrawLine(shootPoint.position, straightPredictPoint, Color.green);
            }
            

            if (Application.isPlaying == true)
            {
                if (Input.GetMouseButtonDown(0) == true && isFiring == false)
                {
                    isFiring = true;
                    Shoot();
                }

                if (isFiring == true)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer = 0f;
                }
            }
        }

        protected void Shoot()
        {
            BulletHandler shootedBullet = type.EnablePool<BulletHandler>(OnBeforeShoot);
        }

        protected void OnBeforeShoot(BulletHandler shootedBullet)
        {
            shootedBullet.transform.position = shootPoint.position;
            shootedBullet.transform.rotation = shootPoint.rotation;
        }

        protected List<Vector3> gizmoPosList = new List<Vector3>();

        protected void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            foreach (Vector3 gizmoPos in gizmoPosList)
            {
                Gizmos.DrawSphere(gizmoPos, 0.1f);
            }
        }
    }
}