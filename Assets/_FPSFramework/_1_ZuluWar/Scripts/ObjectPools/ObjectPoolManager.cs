using FPS_Framework.Pool.Internal;
using Google.GData.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FPS_Framework.Pool
{
    public enum ProjectileType
    {
        _9_SMG_Bullet,
        _105_Cannon_Bullet,
        _57_Shrapnel_Bullet,
        _556_AR_Bullet,
        _577_450_SR_Bullet,
        _762_AR_Bullet,
        _762_SR_Bullet,
        
        _Musket_Bullet,

        _Shrapnel_Piece,
    }

    public enum ImpactType
    {
        _40mm_Explosion_Air,
        _105mm_Explosion,

        BrickImpact,
        ConcreteImpact,
        DirtImpact,
        FleshImpact,
        FoliageImpact,
        GlassImpact,
        MetalImpact,
        PlasterImpact,
        RockImpact,
        WaterImpact,
        WoodImpact,
    }

    public enum FxType
    {
        MuzzleFlashSmoke,
    }

    public enum UnitType
    {
        ZuluWarrior,
        ZuluMusketeer,
    }

    public class ObjectPoolManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[ObjectPoolManager]</b></color> {0}";

        protected static ObjectPoolManager _instance;
        public static ObjectPoolManager Instance
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
        protected EnumerablePooler<ProjectileType> projectilePooler;
        [Space(10)]
        [SerializeField]
        protected EnumerablePooler<ImpactType> impactPooler;
        [Space(10)]
        [SerializeField]
        protected EnumerablePooler<FxType> fxPooler;
        [Space(10)]
        [SerializeField]
        protected EnumerablePooler<UnitType> unitPooler;
        
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("");
                Destroy(this.gameObject);
                return;
            }

            FieldInfo[] fieldInfos = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo info in fieldInfos)
            {
                if (info.FieldType.Name.Contains(nameof(EnumerablePooler)) == true)
                {
                    EnumerablePooler enumerablePooler = info.GetValue(this) as EnumerablePooler;

                    GameObject enumerablePoolerObj = new GameObject("EnumerablePooler_" + enumerablePooler.GetEnumType().Name);
                    enumerablePoolerObj.transform.SetParent(this.transform);

                    enumerablePooler.OnAwake(enumerablePoolerObj.transform);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public PoolHandler GetPoolHandler(ProjectileType type)
        {
            Dictionary<ProjectileType, PoolHandler> targetDictionary = projectilePooler.poolHandlerDic;
            if (targetDictionary.ContainsKey(type) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "PoolHandler not found for type : " + type);
                return null;
            }

            return targetDictionary[type];
        }

        public PoolHandler GetPoolHandler(ImpactType type)
        {
            Dictionary<ImpactType, PoolHandler> targetDictionary = impactPooler.poolHandlerDic;
            if (targetDictionary.ContainsKey(type) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "PoolHandler not found for type : " + type);
                return null;
            }

            return targetDictionary[type];
        }

        public PoolHandler GetPoolHandler(FxType type)
        {
            Dictionary<FxType, PoolHandler> targetDictionary = fxPooler.poolHandlerDic;
            if (targetDictionary.ContainsKey(type) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "PoolHandler not found for type : " + type);
                return null;
            }

            return targetDictionary[type];
        }

        public PoolHandler GetPoolHandler(UnitType type)
        {
            Dictionary<UnitType, PoolHandler> targetDictionary = unitPooler.poolHandlerDic;
            if (targetDictionary.ContainsKey(type) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "PoolHandler not found for type : " + type);
                return null;
            }

            return targetDictionary[type];
        }
    }
}