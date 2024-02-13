using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class BulletHandler : MonoBehaviour
    {
        public enum BulletType
        {
            _556_AR,
            _577_450_SR,
            _762_AR,
            _762_SR,
            _9_SMG,

            _105_Cannon
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

        protected Vector3 enablePos;
        protected Vector3 disablePos;
        protected float flightDistance;

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
            else if (_type == BulletType._105_Cannon)
            {
                bulletName = BulletPoolManager._105_CANNON_BULLET;
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

            enablePos = this.transform.position;

            StartCoroutine(PostOnEnable());
        }

        protected IEnumerator PostOnEnable()
        {
            yield return new WaitForSeconds(lifeTime);

            disablePos = this.transform.position;
            flightDistance = Vector3.Magnitude(enablePos - disablePos);
            BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].ReturnObject(this.gameObject);
        }

        protected List<Collider> collidedList = new List<Collider>();

        protected void OnCollisionEnter(Collision _collision)
        {
            string impactName;
            if (_bulletType == BulletType._105_Cannon)
            {
                impactName = ImpactPoolManager.EXPLOSION_105_CANNON;

                ContactPoint _contactPoint = _collision.GetContact(0);
                Vector3 impactPoint = _contactPoint.point;
                Vector3 impactAngle = _contactPoint.normal + _contactPoint.point;

                GameObject particleObj = ImpactPoolManager.Instance.PoolHandlerDictionary[impactName].EnableObject(impactPoint);
                particleObj.transform.LookAt(impactAngle);

                BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].ReturnObject(this.gameObject);
            }
            else
            {
                if (_collision.collider.TryGetComponent<Impactable>(out Impactable _impactable) == true)
                {
                    Impactable.MaterialType _materialType = _impactable._MaterialType;
                    impactName = Impactable.GetName(_materialType);

                    ContactPoint _contactPoint = _collision.GetContact(0);
                    Vector3 impactPoint = _contactPoint.point;
                    Vector3 impactAngle = _contactPoint.normal + _contactPoint.point;

                    GameObject particleObj = ImpactPoolManager.Instance.PoolHandlerDictionary[impactName].EnableObject(impactPoint);
                    particleObj.transform.LookAt(impactAngle);

                    disablePos = this.transform.position;
                    flightDistance = Vector3.Magnitude(enablePos - disablePos);
                    if (_materialType == Impactable.MaterialType.ShootingTarget)
                    {
                        _impactable._ShootingTarget.ShowText(flightDistance.ToString("F1") + "m");
                    }

                    BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].ReturnObject(this.gameObject);
                }
            }
        }
    }
}