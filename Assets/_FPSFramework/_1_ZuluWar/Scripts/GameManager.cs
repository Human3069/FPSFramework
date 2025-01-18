using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using geniikw.DataRenderer2D;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.HableCurve;

namespace FPS_Framework.ZuluWar
{
    [Serializable]
    public class PhaseCounter
    {
        [SerializeField]
        protected int enemiesPerPhase = 15;

        [Space(10)]
        [ReadOnly]
        public int CurrentPhase = 1;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected int _remainedEnemies = 0;
        protected int RemainedEnemies
        {
            get
            {
                return _remainedEnemies;
            }
            set
            {
                if (_remainedEnemies != value)
                {
                    _remainedEnemies = value;
                    OnRemainedEnemyCountChanged?.Invoke(value);
                }
            }
        }

        public static Action<int> OnRemainedEnemyCountChanged;

        [ReadOnly]
        [SerializeField]
        protected int totalKilledEnemies = 0;

        [Header("Money System")]
        [SerializeField]
        protected int moneyPerKill = 100;
        [SerializeField]
        protected int moneyPerPhase = 500;
        [ReadOnly]
        [SerializeField]
        protected int _totalMoney = 0;
        protected int TotalMoney
        {
            get
            {
                return _totalMoney;
            }
            set
            {
                if (_totalMoney != value)
                {
                    _totalMoney = value;
                    OnTotalMoneyChanged?.Invoke(value);
                }
            }
        }

        public static Action<int> OnTotalMoneyChanged;

        public int EnemiesPerCurrentPhase
        {
            get
            {
                return enemiesPerPhase * CurrentPhase;
            }
        }

        public void OnInit()
        {
            CurrentPhase = 1;

            totalKilledEnemies = 0;
            TotalMoney = 0;
            RemainedEnemies = 0;

            OnRemainedEnemyCountChanged?.Invoke(0);
            OnTotalMoneyChanged?.Invoke(0);
        }

        public void UpdateRemainedCount()
        {
            RemainedEnemies = enemiesPerPhase * CurrentPhase;
        }

        public void UpdatePhase()
        {
            CurrentPhase++;
            TotalMoney += moneyPerPhase;
        }

        public void CountRemainedAndKilled()
        {
            totalKilledEnemies++;
            RemainedEnemies--;

            TotalMoney += moneyPerKill;
        }
    }

    [Serializable]
    public class ArtilleryStrike
    {
        [SerializeField]
        protected float fireDuration = 3f;
        [SerializeField]
        protected float fireInterval = 0.5f;

        [Space(10)]
        [SerializeField]
        protected ProjectileType projectileType;
        [SerializeField]
        protected float randomSphereRadius = 30f;

        public async UniTaskVoid StrikeAsync(Vector3 targetPoint, float beforeDelay)
        {
            await UniTask.WaitForSeconds(beforeDelay);

            bool isFiring = false;
            Vector3 startFiringPoint = targetPoint + (Vector3.up * 300f);

            CountDownAsync().Forget();
            async UniTask CountDownAsync()
            {
                await UniTask.WaitForSeconds(fireDuration);
                isFiring = true;
            }

            while (isFiring == false)
            {
                projectileType.EnablePool(OnBeforePool);
                void OnBeforePool(GameObject bulletObj)
                {
                    bulletObj.transform.position = startFiringPoint + (UnityEngine.Random.insideUnitSphere * randomSphereRadius);
                    bulletObj.transform.forward = Vector3.down;
                }

                await UniTask.WaitForSeconds(fireInterval);
            }
        }
    }

    public class GameManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[GameManager]</b></color> {0}";

        protected static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [SerializeField]
        protected Transform enemySpawnPoint;
        [SerializeField]
        protected float spawnAngle = 180f;
        [SerializeField]
        protected float minSpawnRadius = 250f;
        [SerializeField]
        protected float maxSpawnRadius = 300f;

        [Space(10)]
        public PhaseCounter _PhaseCounter;
        public ArtilleryStrike _ArtilleryStrike;

        [Space(10)]
        public List<DamagedLog> DamagedLogList = new List<DamagedLog>();

        protected List<WarriorController> enemyList = new List<WarriorController>();

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            Debug.Assert(spawnAngle > 0f);
            Debug.Assert(spawnAngle <= 360f);

            Debug.Assert(minSpawnRadius > 0f);
            Debug.Assert(minSpawnRadius <= maxSpawnRadius);

            _PhaseCounter.OnInit();
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                PlayAsync().Forget();
            }
        }

        public async UniTask PlayAsync()
        {
            while (true)
            {
                UI_FadeInOut.Instance.FloatText("Phase " + _PhaseCounter.CurrentPhase, 1f).Forget();
                await UniTask.WaitForSeconds(1f);

                ResetEnemy();

                _PhaseCounter.UpdateRemainedCount();
                await SpawnEnemyAsync(_PhaseCounter.EnemiesPerCurrentPhase);

                await UniTask.WaitUntil(() => enemyList.TrueForAll(x => x.CurrentHealth <= 0f));
                await UniTask.WaitForSeconds(3f);

                _PhaseCounter.UpdatePhase();
            }
        }

        public async UniTask SpawnEnemyAsync(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float startAngle = enemySpawnPoint.eulerAngles.y;
                Vector3 randomizedPos = Vector3Ex.OnRandomCircle(enemySpawnPoint.position, minSpawnRadius, maxSpawnRadius, startAngle, spawnAngle);

                Pool.UnitType.ZuluWarrior.EnablePool((Action<WarriorController>)OnBeforeEnable);
                void OnBeforeEnable(WarriorController controller)
                {
                    controller.transform.position = randomizedPos;
                    controller.SetHeight();

                    enemyList.Add(controller);
                }

                await UniTask.Yield();
            }
        }

        public void ResetEnemy()
        {
            enemyList.ForEach(ForeachPredicate);
            void ForeachPredicate(WarriorController controller)
            {
                controller.ReturnUnit();
            }
        }

        public void CountRemainedAndKilled()
        {
            _PhaseCounter.CountRemainedAndKilled();
        }

#if UNITY_EDITOR
        protected void OnDrawGizmos()
        {
            Handles.color = new Color(1f, 0.5f, 0f, 1f);
            Handles.DrawWireArc(enemySpawnPoint.position, Vector3.up, enemySpawnPoint.forward, spawnAngle, minSpawnRadius);
            Handles.DrawWireArc(enemySpawnPoint.position, Vector3.up, enemySpawnPoint.forward, spawnAngle, maxSpawnRadius);
        }
#endif
    }
}