using _KMH_Framework;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FPS_Framework.ZuluWar.PhaseCounter;

namespace FPS_Framework.ZuluWar
{
    [Serializable]
    public class PhaseCounter
    {
        [SerializedDictionary("UnitType", "PhaseUnitData")]
        public SerializedDictionary<Pool.UnitType, PhaseUnitData> PhaseUnitDataDic;

        [Space(10)]
        [ReadOnly]
        public int CurrentPhase = 1;

        [Header("Money System")]
        [SerializeField]
        protected int moneyPerKill = 100;
        [SerializeField]
        protected int moneyPerPhase = 500;
        [ReadOnly]
        [SerializeField]
        protected int _totalMoney = 0;
        public int TotalMoney
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

        public static Action<int> OnRemainedEnemyCountChanged;
        public static Action<int> OnTotalMoneyChanged;

        public void OnInit()
        {
            CurrentPhase = 1;

            TotalMoney = 0;
            foreach (KeyValuePair<Pool.UnitType, PhaseUnitData> pair in PhaseUnitDataDic)
            {
                pair.Value.OnInit(pair.Key);
            }

            OnRemainedEnemyCountChanged?.Invoke(0);
            OnTotalMoneyChanged?.Invoke(0);
        }

        public void UpdateRemainedCount()
        {
            foreach (KeyValuePair<Pool.UnitType, PhaseUnitData> pair in PhaseUnitDataDic)
            {
                pair.Value.OnUpdateRemainedCount(CurrentPhase);
            }
        }

        public void UpdatePhase()
        {
            CurrentPhase++;
            TotalMoney += moneyPerPhase;
        }

        public void CountRemainedAndKilled(Pool.UnitType unitType)
        {
            PhaseUnitDataDic[unitType].OnCountRemainedAndKilled();
            TotalMoney += moneyPerKill;
        }

        [Serializable]
        public class PhaseUnitData
        {
            private Pool.UnitType unitType;

            [SerializeField]
            private int UnitPerPhase;
            [ReadOnly]
            [SerializeField]
            private int _remainedEnemies;
            public int RemainedEnemies
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

                        int totalRemainedCount = 0;
                        foreach (KeyValuePair<Pool.UnitType, PhaseUnitData> pair in GameManager.Instance._PhaseCounter.PhaseUnitDataDic)
                        {
                            totalRemainedCount += pair.Value.RemainedEnemies;
                        }

                        OnRemainedEnemyCountChanged?.Invoke(totalRemainedCount);
                    }
                }
            }

            public void OnInit(Pool.UnitType unitType)
            {
                this.unitType = unitType;
                RemainedEnemies = 0;
            }

            public void OnUpdateRemainedCount(int currentPhase)
            {
                RemainedEnemies = currentPhase * UnitPerPhase;
            }

            public void OnCountRemainedAndKilled()
            {
                RemainedEnemies--;
            }

            public async UniTask SpawnAsync(int currentPhase, Vector3 center, float minRadius, float maxRadius, float startAngle, float totalAngle)
            {
                for (int i = 0; i < currentPhase * UnitPerPhase; i++)
                {
                    Vector3 randomizedPos = Vector3Ex.OnRandomCircle(center, minRadius, maxRadius, startAngle, totalAngle);

                    unitType.EnablePool<MeleeWarriorController>(OnBeforeEnable);
                    void OnBeforeEnable(MeleeWarriorController controller)
                    {
                        controller.transform.position = randomizedPos;
                        controller.SetHeight();

                        GameManager.Instance.EnemyList.Add(controller);
                    }

                    await UniTask.Yield();
                }
            }
        }
    }

    [Serializable]
    public class ArtilleryStrike
    {
        private const float DEFORE_DELAY_TIME = 5f;

        [SerializeField]
        protected float fireDuration = 3f;
        protected int fireDurationUpgradeCount = 0;

        [SerializeField]
        protected float fireInterval = 0.5f;
        protected int fireIntervalUpgradeCount = 0;

        [SerializeField]
        protected float fireRadius = 30f;
        protected int fireRadiusUpgradeCount = 0;

        [Space(10)]
        [SerializeField]
        protected ProjectileType projectileType;

        public async UniTaskVoid StrikeAsync(Vector3 targetPoint)
        {
            await UniTask.WaitForSeconds(DEFORE_DELAY_TIME);

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
                    bulletObj.transform.position = startFiringPoint + (UnityEngine.Random.insideUnitSphere * fireRadius);
                    bulletObj.transform.forward = Vector3.down;
                }

                await UniTask.WaitForSeconds(fireInterval);
            }
        }

        public float GetFireDuration()
        {
            return fireDuration;
        }

        public float GetFireInterval()
        {
            return fireInterval;
        }

        public float GetFireRadius()
        {
            return fireRadius;
        }

        public float UpgradeFireDuration(out int upgradeCount)
        {
            fireDurationUpgradeCount++;
            fireDuration += 0.1f;

            upgradeCount = fireDurationUpgradeCount;
            return fireDuration;
        }

        public float UpgradeFireInterval(out int upgradeCount)
        {
            fireIntervalUpgradeCount++;
            fireInterval *= 0.975f;

            upgradeCount = fireIntervalUpgradeCount;
            return fireInterval;
        }

        public float UpgradeFireRadius(out int upgradeCount)
        {
            fireRadiusUpgradeCount++;
            fireRadius += 1f;

            upgradeCount = fireIntervalUpgradeCount;
            return fireRadius;
        }
    }

    [Serializable]
    public class UnitStats
    {
        public delegate void Upgraded(float riflemanRange, float riflemanFireInterval, float riflemanAccuracy);
        public static event Upgraded OnUpgraded;

        [SerializeField]
        protected float riflemanRange = 50f;
        protected int riflemanRangeUpgradeCount = 0;

        [SerializeField]
        protected float riflemanFireInterval = 30f;
        protected int riflemanFireIntervalUpgradeCount = 0;

        [SerializeField]
        protected float riflemanAccuracy = 0.5f;
        protected int riflemanAccuracyUpgradeCount = 0;

        public float GetRange()
        {
            return riflemanRange;
        }

        public float GetFireInterval()
        {
            return riflemanFireInterval;
        }

        public float GetAccuracy()
        {
            return riflemanAccuracy;
        }

        public float UpgradeRange(out int upgradeCount)
        {
            riflemanRangeUpgradeCount++;
            riflemanRange += 1.5f;

            upgradeCount = riflemanRangeUpgradeCount;
            OnUpgraded?.Invoke(riflemanRange, riflemanFireInterval, riflemanAccuracy);

            return riflemanRange;
        }

        public float UpgradeFireInterval(out int upgradeCount)
        {
            riflemanFireIntervalUpgradeCount++;
            riflemanFireInterval *= 0.98f;

            upgradeCount = riflemanFireIntervalUpgradeCount;
            OnUpgraded?.Invoke(riflemanRange, riflemanFireInterval, riflemanAccuracy);

            return riflemanFireInterval;
        }

        public float UpgradeAccuracy(out int upgradeCount)
        {
            riflemanAccuracyUpgradeCount++;
            riflemanAccuracy = Mathf.Pow(riflemanAccuracy, 0.985f);

            upgradeCount = riflemanAccuracyUpgradeCount;
            OnUpgraded?.Invoke(riflemanRange, riflemanFireInterval, riflemanAccuracy);

            return riflemanAccuracy;
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
        public UnitStats _UnitStats;

        [Space(10)]
        public List<DamagedLog> DamagedLogList = new List<DamagedLog>();

        public List<IDamageable> EnemyList = new List<IDamageable>();

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
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                _UnitStats.UpgradeAccuracy(out _);
            }
        }

        public async UniTask PlayAsync()
        {
            while (true)
            {
                UI_FadeInOut.Instance.FloatText("Phase " + _PhaseCounter.CurrentPhase, 1f).Forget();
                await UniTask.WaitForSeconds(1f);

                EnemyList.Clear();
                ResetEnemy();
                _PhaseCounter.UpdateRemainedCount();

                foreach (KeyValuePair<Pool.UnitType, PhaseUnitData> pair in _PhaseCounter.PhaseUnitDataDic)
                {
                    await pair.Value.SpawnAsync(_PhaseCounter.CurrentPhase, enemySpawnPoint.position, minSpawnRadius, maxSpawnRadius, enemySpawnPoint.eulerAngles.y, spawnAngle);
                }

                await UniTask.WaitUntil(() => EnemyList.TrueForAll(x => x.CurrentHealth <= 0f));
                await UniTask.WaitForSeconds(3f);

                _PhaseCounter.UpdatePhase();
            }
        }

        public void ResetEnemy()
        {
            EnemyList.ForEach(ForeachPredicate);
            void ForeachPredicate(IDamageable enemyDamageable)
            {
                enemyDamageable.ReturnPool();
            }
        }

        public void CountRemainedAndKilled(Pool.UnitType unitType)
        {
            _PhaseCounter.CountRemainedAndKilled(unitType);
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