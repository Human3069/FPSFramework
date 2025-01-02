using System.Collections.Generic;
using System;
using UnityEngine;

namespace FPS_Framework.Pool.Internal
{
    public abstract class EnumerablePooler
    {
        public abstract void OnAwake(Transform targetTransform);

        public abstract Type GetEnumType();
    }

    [Serializable]
    public class EnumerablePooler<TEnum> : EnumerablePooler where TEnum : struct, IConvertible
    {
        private const string LOG_FORMAT = "<color=white><b>[EnumerablePooler]</b></color> {0}";

        [SerializeField]
        protected List<PoolHandler> _poolHandlerList = new List<PoolHandler>();
        internal Dictionary<TEnum, PoolHandler> poolHandlerDic = new Dictionary<TEnum, PoolHandler>();

        public override void OnAwake(Transform targetTransform)
        {
            foreach (PoolHandler handler in _poolHandlerList)
            {
                if (Enum.TryParse(handler.Prefab.name, out TEnum enumValue) == false)
                {
                    Debug.LogErrorFormat(LOG_FORMAT, "enumValue name is not specified : " + handler.Prefab.name);
                    continue;
                }

                poolHandlerDic.Add(enumValue, handler);

                GameObject poolHandlerParent = new GameObject("PoolHandler_" + handler.Prefab.name);
                poolHandlerParent.transform.SetParent(targetTransform);

                handler.Initialize(poolHandlerParent.transform);
            }
        }

        public PoolHandler GetPoolHandler(TEnum enumValue)
        {
            if (poolHandlerDic.ContainsKey(enumValue) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "PoolHandler not found for type : " + enumValue);
                return null;
            }

            return poolHandlerDic[enumValue];
        }

        public override Type GetEnumType()
        {
            return typeof(TEnum);
        }
    }
}