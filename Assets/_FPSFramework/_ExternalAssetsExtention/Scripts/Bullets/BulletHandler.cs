using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

namespace FPSFramework
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class BulletHandler : MonoBehaviour
    {
        public enum BulletType
        {
            _556,
            _577_450,
            _762,
            _9
        }

        [SerializeField]
        protected BulletType _bulletType;
        protected string bulletName;

        [Space(10)]
        [SerializeField]
        protected float lifeTime;
        [SerializeField]
        protected float speed;

        protected Rigidbody _rigidbody;

        public static string GetName(BulletType _type)
        {
            string bulletName;
            if (_type == BulletType._556)
            {
                bulletName = BulletPoolManager._556_BULLET;
            }
            else if (_type == BulletType._577_450)
            {
                bulletName = BulletPoolManager._577_450_BULLET;
            }
            else if (_type == BulletType._762)
            {
                bulletName = BulletPoolManager._762_BULLET;
            }
            else if (_type == BulletType._9)
            {
                bulletName = BulletPoolManager._9_BULLET;
            }
            else
            {
                bulletName = null;
                Debug.Assert(false);
            }

            return bulletName;
        }

        protected void Awake()
        {
            _rigidbody = this.GetComponent<Rigidbody>();

            bulletName = GetName(_bulletType);
        }

        protected void OnEnable()
        {
            _rigidbody.velocity = this.transform.forward * speed;
            _rigidbody.angularVelocity = Vector3.zero;

            StartCoroutine(PostOnEnable());
        }

        protected IEnumerator PostOnEnable()
        {
            yield return new WaitForSeconds(lifeTime);

            BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].ReturnObject(this.gameObject);
        }

        protected List<Collider> collidedList = new List<Collider>();

        protected void OnCollisionEnter(Collision _collision)
        {
            if (_collision.collider.TryGetComponent<Impactable>(out Impactable _impactable) == true)
            {
                Impactable.MaterialType _materialType = _impactable._MaterialType;
                string impactName = Impactable.GetName(_materialType);

                ContactPoint _contactPoint = _collision.GetContact(0);
                Vector3 impactPoint = _contactPoint.point;
                Vector3 impactAngle = _contactPoint.normal + _contactPoint.point;

                GameObject particleObj = ImpactPoolManager.Instance.PoolHandlerDictionary[impactName].EnableObject(impactPoint);
                particleObj.transform.LookAt(impactAngle);

                BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].ReturnObject(this.gameObject);
            }
        }
    }
}