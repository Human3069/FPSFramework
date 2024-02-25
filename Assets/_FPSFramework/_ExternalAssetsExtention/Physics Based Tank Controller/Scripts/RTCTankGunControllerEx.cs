#pragma warning disable 0414

using UnityEngine;
using System.Collections;
using _KMH_Framework;

namespace FPS_Framework
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HingeJoint))]
    [RequireComponent(typeof(AudioSource))]

    public class RTCTankGunControllerEx : MonoBehaviour
    {
        protected Rigidbody _rigidbody;
        protected AudioSource _audioSource;
        protected RTCTankControllerEx tankController;

        [SerializeField]
        protected TankCameraController tankCameraController;

        [SerializeField]
        protected bool canControl = true;

        [SerializeField]
        protected GameObject barrel;
        [SerializeField]
        protected Transform barrelOut;

        protected HingeJoint _hingeJoint;
        protected JointLimits jointRotationLimit;

        protected float inputSteer;

        [Space(10)]
        [SerializeField]
        protected int rotationTorque = 1000;
        [SerializeField]
        protected float maximumAngularVelocity = 1.5f;
        [SerializeField]
        protected int maximumRotationLimit = 160;
        [SerializeField]
        protected float minimumElevationLimit = 10;
        [SerializeField]
        protected float maximumElevationLimit = 25;
        [SerializeField]
        protected bool useLimitsForRotation = true;

        protected float rotationVelocity;

        [ReadOnly]
        [SerializeField]
        protected float rotationOfTheGun;

        protected Transform target;

        [Header("Bullets")]
        [SerializeField]
        protected int recoilForce = 10000;

        [ReadOnly]
        [SerializeField]
        protected int ammoCount = 15;

        [SerializeField]
        protected float reloadTime = 3f;

        [ReadOnly]
        [SerializeField]
        protected bool isShootable = true;

        protected bool _isFireSubGun;
        protected bool IsFireSubGun
        {
            get
            {
                return _isFireSubGun;
            }
            set
            {
                if (_isFireSubGun != value)
                {
                    _isFireSubGun = value;

                    if (value == true)
                    {
                        StartCoroutine(PostFireSubGunValueTrue());
                    }
                }
            }
        }

        protected IEnumerator PostFireSubGunValueTrue()
        {
            while (IsFireSubGun == true)
            {
                FireSubGun();

                yield return new WaitForSeconds(0.1f);
            }
        }

        protected AudioSource fireAudioSource;

        [SerializeField]
        protected AudioClip cannonSoundClip;
        [SerializeField]
        protected AudioClip machineGunSoundClip;

        [Space(10)]
        [SerializeField]
        protected ParticleSystem muzzleFlashParticle;
        [SerializeField]
        protected ParticleSystemForceField forceField;

        protected virtual void Awake()
        {
            Debug.Assert(tankCameraController != null);

            forceField.enabled = false;

            _rigidbody = this.GetComponent<Rigidbody>();
            _audioSource = this.GetComponent<AudioSource>();
            tankController = transform.root.gameObject.GetComponent<RTCTankControllerEx>();

            GameObject newTarget = new GameObject("Target");
            target = newTarget.transform;

            _rigidbody.maxAngularVelocity = maximumAngularVelocity;
            _rigidbody.interpolation = RigidbodyInterpolation.None;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            _hingeJoint = GetComponent<HingeJoint>();
        }

        protected virtual void OnEnable()
        {
            tankCameraController.enabled = true;
        }

        protected virtual void OnDisable()
        {
            tankCameraController.enabled = false;
        }

        protected virtual void Update()
        {
            if (tankController.canControl == false ||
                canControl == false)
            {
                return;
            }

            FireMainGun();
            IsFireSubGun = Input.GetButton("Fire2");
            JointConfiguration();
        }

        protected virtual void FixedUpdate()
        {
            if (tankController.canControl == false ||
               canControl == false)
            {
                return;
            }

            if (transform.localEulerAngles.y > 0 && transform.localEulerAngles.y < 180)
            {
                rotationOfTheGun = transform.localEulerAngles.y;
            }
            else
            {
                rotationOfTheGun = transform.localEulerAngles.y - 360;
            }

            Vector3 targetPosition = transform.InverseTransformPoint(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));

            inputSteer = (targetPosition.x / targetPosition.magnitude);
            rotationVelocity = _rigidbody.angularVelocity.y;

            _rigidbody.AddRelativeTorque(0, (rotationTorque) * inputSteer, 0, ForceMode.Force);

            Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            barrel.transform.rotation = Quaternion.Slerp(barrel.transform.rotation, targetRotation, Time.deltaTime * 4f);

            if (barrel.transform.localEulerAngles.x > 0 && barrel.transform.localEulerAngles.x < 180)
            {
                barrel.transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Clamp(barrel.transform.localEulerAngles.x, -360, minimumElevationLimit), 0, 0));
            }

            if (barrel.transform.localEulerAngles.x > 180 && barrel.transform.localEulerAngles.x < 360)
            {
                barrel.transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Clamp(barrel.transform.localEulerAngles.x - 360, -maximumElevationLimit, 360), 0, 0));
            }
        }

        protected virtual void FireMainGun()
        {
            target.position = tankCameraController.transform.position + (tankCameraController.transform.forward * 100);

            if (Input.GetButtonDown("Fire1") &&
                isShootable == true &&
                ammoCount > 0)
            {
                BulletPoolManager.Instance.PoolHandlerDictionary[BulletPoolManager._105_CANNON_BULLET].EnableObject(barrelOut);

                muzzleFlashParticle.Play();
                _rigidbody.AddForce(-transform.forward * recoilForce, ForceMode.VelocityChange);
                _audioSource.PlayOneShot(cannonSoundClip);
                ammoCount--;

                StartCoroutine(FireDelayRoutine());
                StartCoroutine(ParticleForceFieldRoutine());
            }
        }

        protected IEnumerator FireDelayRoutine()
        {
            isShootable = false;
            yield return new WaitForSeconds(reloadTime);
            if (ammoCount > 0)
            {
                isShootable = true;
            }
        }

        protected IEnumerator ParticleForceFieldRoutine()
        {
            forceField.enabled = true;
            yield return new WaitForSeconds(0.1f);
            forceField.enabled = false;
        }

        protected virtual void FireSubGun()
        {
            BulletPoolManager.Instance.PoolHandlerDictionary[BulletPoolManager._762_SR_BULLET].EnableObject(barrelOut);
            _audioSource.PlayOneShot(machineGunSoundClip);
        }

        protected virtual void JointConfiguration()
        {
            if (useLimitsForRotation == true)
            {
                jointRotationLimit.min = -maximumRotationLimit;
                jointRotationLimit.max = maximumRotationLimit;

                _hingeJoint.limits = jointRotationLimit;
                _hingeJoint.useLimits = true;
            }
            else
            {
                _hingeJoint.useLimits = false;
            }
        }
    }
}