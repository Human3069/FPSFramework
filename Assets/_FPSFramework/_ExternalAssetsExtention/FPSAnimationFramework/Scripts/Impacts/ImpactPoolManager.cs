using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public class ImpactPoolManager : BaseObjectPoolManager<ImpactPoolHandler>
    {
        private const string LOG_FORMAT = "<color=white><b>[ImpactPoolManager]</b></color> {0}";

        public static string BRICK_IMPACT;
        public static string CONCRETE_IMPACT;
        public static string DIRT_IMPACT;
        public static string FOLIAGE_IMPACT;
        public static string GLASS_IMPACT;
        public static string METAL_IMPACT;
        public static string PLASTER_IMPACT;
        public static string ROCK_IMPACT;
        public static string WATER_IMPACT;
        public static string WOOD_IMPACT;

        public static string EXPLOSION_105_CANNON;

        public static new ImpactPoolManager Instance
        {
            get
            {
                return _instance as ImpactPoolManager;
            }
            protected set
            {
                _instance = value;
            }
        }

        protected bool _isReady = false;
        public bool IsReady
        {
            get
            {
                return _isReady;
            }
            protected set
            {
                _isReady = value;
            }
        }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            BRICK_IMPACT = _InitInfos[0].Obj.name;
            CONCRETE_IMPACT = _InitInfos[1].Obj.name;
            DIRT_IMPACT = _InitInfos[2].Obj.name;
            FOLIAGE_IMPACT = _InitInfos[3].Obj.name;
            GLASS_IMPACT = _InitInfos[4].Obj.name;
            METAL_IMPACT = _InitInfos[5].Obj.name;
            PLASTER_IMPACT = _InitInfos[6].Obj.name;
            ROCK_IMPACT = _InitInfos[7].Obj.name;
            WATER_IMPACT = _InitInfos[8].Obj.name;
            WOOD_IMPACT = _InitInfos[9].Obj.name;

            EXPLOSION_105_CANNON = _InitInfos[10].Obj.name;

            for (int i = 0; i < _InitInfos.Length; i++)
            {
                string objName = _InitInfos[i].Obj.name;
                GameObject newObj = new GameObject(objName + "PoolHandler");
                newObj.transform.SetParent(this.transform);

                ImpactPoolHandler newHandler = newObj.AddComponent<ImpactPoolHandler>();
                newHandler.ThisInitIndex = i;
                newHandler.Initialize();

                PoolHandlerDictionary.Add(objName, newHandler);
            }

            IsReady = true;
        }
    }

    public class ImpactPoolHandler : BaseObjectPoolHandler
    {
        // private const string LOG_FORMAT = "<color=white><b>[ImpactPoolHandler]</b></color> {0}";

        public override void Initialize()
        {
            enableObjectsParent = new GameObject("Enables");
            enableObjectsParent.transform.SetParent(this.transform);

            disableObjectsParent = new GameObject("Disables");
            disableObjectsParent.transform.SetParent(this.transform);

            for (int i = 0; i < ImpactPoolManager.Instance._InitInfos[ThisInitIndex].ObjInitCount; i++)
            {
                poolingQueue.Enqueue(CreateNewObject());
            }
        }

        protected override GameObject CreateNewObject()
        {
            GameObject newObj = Instantiate(ImpactPoolManager.Instance._InitInfos[ThisInitIndex].Obj);
            newObj.transform.SetParent(disableObjectsParent.transform);
            newObj.gameObject.SetActive(false);

            return newObj;
        }

        public override GameObject EnableObject()
        {
            if (poolingQueue.Count > 0)
            {
                GameObject obj = poolingQueue.Dequeue();
                obj.transform.SetParent(enableObjectsParent.transform);
                obj.gameObject.SetActive(true);

                return obj;
            }
            else
            {
                GameObject newObj = CreateNewObject();
                newObj.transform.SetParent(enableObjectsParent.transform);
                newObj.gameObject.SetActive(true);

                return newObj;
            }
        }

        public override GameObject EnableObject(Transform _transform)
        {
            if (poolingQueue.Count > 0)
            {
                GameObject obj = poolingQueue.Dequeue();

                obj.transform.SetPositionAndRotation(_transform.position, _transform.rotation);
                obj.transform.SetParent(enableObjectsParent.transform);
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                GameObject newObj = CreateNewObject();

                newObj.transform.SetPositionAndRotation(_transform.position, _transform.rotation);
                newObj.transform.SetParent(enableObjectsParent.transform);
                newObj.gameObject.SetActive(true);

                return newObj;
            }
        }

        public override GameObject EnableObject(Vector3 _position, Quaternion _rotation)
        {
            if (poolingQueue.Count > 0)
            {
                GameObject obj = poolingQueue.Dequeue();

                obj.transform.SetPositionAndRotation(_position, _rotation);
                obj.transform.SetParent(enableObjectsParent.transform);
                obj.gameObject.SetActive(true);

                return obj;
            }
            else
            {
                GameObject newObj = CreateNewObject();

                newObj.transform.SetPositionAndRotation(_position, _rotation);
                newObj.transform.SetParent(enableObjectsParent.transform);
                newObj.gameObject.SetActive(true);

                return newObj;
            }
        }

        public virtual GameObject EnableObject(Vector3 _position)
        {
            if (poolingQueue.Count > 0)
            {
                GameObject obj = poolingQueue.Dequeue();

                obj.transform.position = _position;
                obj.transform.SetParent(enableObjectsParent.transform);
                obj.gameObject.SetActive(true);
                return obj;
            }
            else
            {
                GameObject newObj = CreateNewObject();

                newObj.transform.position = _position;
                newObj.transform.SetParent(enableObjectsParent.transform);
                newObj.gameObject.SetActive(true);

                return newObj;
            }
        }

        public override void ReturnObject(GameObject pool)
        {
            pool.gameObject.SetActive(false);
            pool.transform.SetParent(disableObjectsParent.transform);
            poolingQueue.Enqueue(pool);
        }
    }
}