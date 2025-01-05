using _KMH_Framework;
using FPS_Framework.Pool;
using FPS_Framework.ZuluWar;
using UnityEngine;

namespace FPS_Framework
{
    public class SplashBulletHandler : BulletHandler
    {
        [Header("=== SplashBulletHandler ===")]
        [SerializeField]
        protected float splashMaxRadius = 10f;
        [SerializeField]
        protected ImpactType explosionImpactType;

        protected override void OnHit(RaycastHit[] hits)
        {
            Vector3 impactPoint = hits[0].point;

            explosionImpactType.EnablePool(obj => obj.transform.position = impactPoint);
            this.gameObject.ReturnPool(projectileType);

            Collider[] colliders = Physics.OverlapSphere(impactPoint, splashMaxRadius);
            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent<WarriorController>(out WarriorController warrior) == true)
                {
                    float distance = (warrior.transform.position - impactPoint).magnitude;
                    float splashDamage = Mathf.Lerp(damage, 0f, distance / splashMaxRadius);

                    warrior.CurrentHealth -= splashDamage;
                }
            }

            if (hits[0].collider.TryGetComponent<Rigidbody>(out Rigidbody _rigidbody) == true)
            {
                float distance = (_rigidbody.transform.position - impactPoint).magnitude;
                float explosionForce = Mathf.Lerp(130000f, 0f, distance / splashMaxRadius);

                _rigidbody.isKinematic = false;
                _rigidbody.AddExplosionForce(explosionForce, impactPoint, splashMaxRadius, 0.3f);
            }
        }
    }
}