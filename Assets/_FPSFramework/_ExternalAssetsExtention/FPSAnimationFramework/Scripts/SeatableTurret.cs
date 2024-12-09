using _KMH_Framework;
using Cysharp.Threading.Tasks;
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

                    OnSeatedAsync().Forget();
                }
            }
        }

        [Header("SeatableTurrent")]
        [SerializeField]
        protected Transform firePos;
        [SerializeField]
        protected Camera targetCamera;

        [Space(10)]
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

        protected bool isFireRating = false;

        protected void Awake()
        {
            audioSource = this.GetComponent<AudioSource>();
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

                float scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta != 0)
                {
                    currentPredictRange = Mathf.Clamp(currentPredictRange + scrollDelta * predictRangeDelta, predictRange.x, predictRange.y);
                    predictRangeText.text = currentPredictRange + "m";
                }

                await UniTask.Yield();
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

        protected void Update()
        {
            if (IsSeated == true)
            {
                Vector3 predictPos = Vector3Ex.GetPredictPositionOnGravity(firePos.position, firePos.position + firePos.forward * currentPredictRange, Vector3.zero, 400f);
                Vector3 screenPos = targetCamera.WorldToScreenPoint(predictPos);
                predictCrosshairRect.position = screenPos;
            }
        }
    }
}