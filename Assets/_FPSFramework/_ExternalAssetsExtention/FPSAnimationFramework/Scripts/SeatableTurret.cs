using _KMH_Framework;
using Cinemachine;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static FPS_Framework.BulletHandler;

namespace FPS_Framework
{
    public class SeatableTurret : Seat
    {
        protected AudioSource audioSource;

        public override bool IsSeated
        {
            get
            {
                return _isSeated;
            }
            protected set
            {
                if (_isSeated != value)
                {
                    _isSeated = value;

                    if (value == true)
                    {
                        OnSeatedAsync().Forget();
                    }
                    else
                    {
                        isZoom = false;
                        vCam.m_Lens.FieldOfView = 60f;
                    }
                }
            }
        }

        [Header("SeatableTurrent")]
        [SerializeField]
        protected bool isRotateToKey = true;
        [SerializeField]
        protected Transform firePos;
        [SerializeField]
        protected Camera targetCamera;
        [SerializeField]
        protected CinemachineVirtualCamera vCam;

        [Space(10)]
        [SerializeField]
        protected LayerMask raycastMask;
        [SerializeField]
        protected float predictableDrag;
        [SerializeField]
        protected float predictableSpeed;
        [SerializeField]
        protected float predictAccuracy = 0.9f;
        [SerializeField]
        protected int predictIterationCount = 150;
        [ReadOnly]
        [SerializeField]
        protected float predictDistance;

        [Space(10)]
        [SerializeField]
        protected RectTransform predictCrosshair;
        [SerializeField]
        protected TextMeshProUGUI predictDistanceText;

        [Space(10)]
        [SerializeField]
        protected ProjectileType projectileType;

        [Space(10)]
        [SerializeField]
        protected float fireRate = 0.3f;
        [SerializeField]
        protected float fireRandomAngle = 1f;
        [SerializeField]
        protected int firePerShot = 1;

        [SerializeField]
        protected AudioClip[] fireClips;

        [Space(10)]
        [SerializeField]
        protected float rotateSpeed = 2f;

        protected bool isFireRating = false;
        protected bool isZoom = false;

        protected void Awake()
        {
            audioSource = this.GetComponent<AudioSource>();
        }
    
        protected virtual async UniTaskVoid OnSeatedAsync()
        {
            while (IsSeated == true)
            {
                if (Input.GetMouseButtonDown(0) == true && isFireRating == false)
                {
                    FireAsync().Forget();
                }

                if (isRotateToKey == true)
                {
                    float actualRotateSpeed = rotateSpeed * Time.deltaTime;
                    if (KeyInputManager.Instance.KeyData["Move Forward"].IsInput == true)
                    {
                        this.transform.Rotate(-actualRotateSpeed, 0f, 0f);
                    }
                    else if (KeyInputManager.Instance.KeyData["Move Backward"].IsInput == true)
                    {
                        this.transform.Rotate(actualRotateSpeed, 0f, 0f);
                    }

                    if (KeyInputManager.Instance.KeyData["Move Right"].IsInput == true)
                    {
                        this.transform.Rotate(0f, actualRotateSpeed, 0f);
                    }
                    else if (KeyInputManager.Instance.KeyData["Move Left"].IsInput == true)
                    {
                        this.transform.Rotate(0f, -actualRotateSpeed, 0f);
                    }
                }
                else
                {
                    // this.transform.transform.Rotate(-Input.GetAxis("Mouse Y") * rotateSpeed, Input.GetAxis("Mouse X") * rotateSpeed, 0f);
                    this.transform.forward = Vector3.Lerp(this.transform.forward, targetCamera.transform.forward, rotateSpeed * Time.deltaTime);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    isZoom = !isZoom;
                    if (isZoom == true)
                    {
                        vCam.m_Lens.FieldOfView = 20f;
                    }
                    else
                    {
                        vCam.m_Lens.FieldOfView = 60f;
                    }
                }

                Vector3 predictedHitPos = Predictor.PredictWithSingleCollision(predictableDrag, firePos.forward * predictableSpeed, raycastMask, out predictDistance, firePos.position, accuracy: predictAccuracy, iterationLimit: predictIterationCount);
                Vector3 crosshairScreenPos = targetCamera.WorldToScreenPoint(predictedHitPos);
                predictCrosshair.position = crosshairScreenPos;
                predictDistanceText.text = predictDistance.ToString("F0") + "m";

                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }
        }

        protected virtual async UniTaskVoid FireAsync()
        {
            while (IsSeated == true && Input.GetMouseButton(0) == true)
            {
                isFireRating = true;

                for (int i = 0; i < firePerShot; i++)
                {
                    Vector3 randomedEuler = firePos.eulerAngles + new Vector3(Random.Range(-fireRandomAngle, fireRandomAngle), Random.Range(-fireRandomAngle, fireRandomAngle), 0);

                    projectileType.EnablePool<BulletHandler>(OnBeforeEnablePool);
                    void OnBeforeEnablePool(BulletHandler bulletHandler)
                    {
                        bulletHandler.transform.position = firePos.position;
                        bulletHandler.transform.rotation = Quaternion.Euler(randomedEuler);
                    }
                }

                int randomIndex = Random.Range(0, fireClips.Length);
                audioSource.PlayOneShot(fireClips[randomIndex]);

                await UniTask.WaitForSeconds(fireRate);
                isFireRating = false;
            }
        }

        protected void OnDrawGizmos()
        {
            if (IsSeated == true)
            {
                Gizmos.color = Color.red;

                List<Vector3> predictedPosList = Predictor.PredictWithoutCollision(predictableDrag, firePos.forward * predictableSpeed, out _, firePos.position, accuracy: predictAccuracy, iterationLimit: predictIterationCount);
                for (int i = 0; i < predictedPosList.Count; i++)
                {
                    if (i != 0)
                    {
                        Gizmos.DrawLine(predictedPosList[i - 1], predictedPosList[i]);
                    }
                }

                Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

                Vector3 predictedHitPos = Predictor.PredictWithSingleCollision(predictableDrag, firePos.forward * predictableSpeed, raycastMask, out _, firePos.position, accuracy: predictAccuracy, iterationLimit: predictIterationCount);
                Gizmos.DrawSphere(predictedHitPos, 1f);
            }
        }
    }
}