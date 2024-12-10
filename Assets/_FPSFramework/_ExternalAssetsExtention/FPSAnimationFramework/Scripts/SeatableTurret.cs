using _KMH_Framework;
using Cinemachine;
using Cysharp.Threading.Tasks;
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
        protected bool isPredictStaticHit = false;
        [SerializeField]
        protected Vector2Int predictRange = new Vector2Int(50, 500);
        [ReadOnly]
        [SerializeField]
        protected float currentPredictRange = 200f;
        [SerializeField]
        protected float predictRangeDelta = 50f;
        [SerializeField]
        protected RectTransform predictCrosshairRect;
        [SerializeField]
        protected TextMeshProUGUI predictRangeText;

        [Space(10)]
        [SerializeField]
        protected BulletType bulletType;
        protected string bulletName;

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

        protected static Dictionary<BulletType, BulletPredictData> predictDic = new Dictionary<BulletType, BulletPredictData>();
        protected bool isFireRating = false;
        protected bool isZoom = false;

        protected void Awake()
        {
            audioSource = this.GetComponent<AudioSource>();
            if (predictDic.ContainsKey(bulletType) == false || predictDic[bulletType] == null)
            {
                predictDic[bulletType] = Resources.Load<BulletPredictData>(bulletType.ToString() + "_PredictData");
            }
            
            PostAwake().Forget();
        }
    
        protected async UniTaskVoid PostAwake()
        {
            await UniTask.WaitUntil(() => BulletPoolManager.Instance.IsReady == true);
            bulletName = BulletHandler.GetName(bulletType);
        }

        protected virtual async UniTaskVoid OnSeatedAsync()
        {
            predictRangeText.text = currentPredictRange + "m";

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

                DrawPredict();

                await UniTask.Yield();
            }
        }

        protected void DrawPredict()
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            if (scrollDelta != 0)
            {
                currentPredictRange = Mathf.Clamp(currentPredictRange + scrollDelta * predictRangeDelta, predictRange.x, predictRange.y);
                predictRangeText.text = currentPredictRange + "m";
            }

            Predict[] predicts = predictDic[bulletType].Predicts;
            if (isPredictStaticHit == true)
            {
                bool isFoundStatic = false;
                Vector3 staticHitPoint = Vector3.zero;

                for (int i = 0; i < predicts.Length; i++)
                {
                    if (i != 0)
                    {
                        Vector3 startPos = firePos.transform.position
                                         + (firePos.transform.forward * predicts[i - 1].zDirection)
                                         - (firePos.transform.up * predicts[i - 1].yDirection);

                        Vector3 endPos = firePos.transform.position
                                         + (firePos.transform.forward * predicts[i].zDirection)
                                         - (firePos.transform.up * predicts[i].yDirection);

                        Vector3 direction = (endPos - startPos).normalized;
                        float length = (endPos - startPos).magnitude;

                        RaycastHit[] hits = Physics.RaycastAll(startPos, direction, length);
                        foreach (RaycastHit hit in hits)
                        {
                            if (hit.collider.gameObject.isStatic == true)
                            {
                                isFoundStatic = true;
                                staticHitPoint = hit.point;

                                break;
                            }
                        }

                        if (isFoundStatic == true)
                        {
                            break;
                        }
                    }
                }

                if (isFoundStatic == true)
                {
                    Vector3 screenPredictPos = targetCamera.WorldToScreenPoint(staticHitPoint);
                    predictCrosshairRect.position = screenPredictPos;
                }
            }
            else
            {
                Predict foundPredict = new Predict();

                for (int i = 0; i < predicts.Length; i++)
                {
                    if (predicts[i].Magnitude > currentPredictRange)
                    {
                        foundPredict = predicts[i];
                        break;
                    }
                }

                Vector3 worldPredictPos = firePos.transform.position
                                        + (firePos.transform.forward * foundPredict.zDirection)
                                        - (firePos.transform.up * foundPredict.yDirection);
                Vector3 screenPredictPos = targetCamera.WorldToScreenPoint(worldPredictPos);
                predictCrosshairRect.position = screenPredictPos;
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
                    BulletPoolManager.Instance.PoolHandlerDictionary[bulletName].EnableObject(firePos.position, Quaternion.Euler(randomedEuler));
                }

                int randomIndex = Random.Range(0, fireClips.Length);
                audioSource.PlayOneShot(fireClips[randomIndex]);

                await UniTask.WaitForSeconds(fireRate);
                isFireRating = false;
            }
        }
    }
}