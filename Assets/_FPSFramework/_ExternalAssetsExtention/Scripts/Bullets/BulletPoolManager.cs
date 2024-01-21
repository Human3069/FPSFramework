using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public class BulletPoolManager : BaseObjectPoolManager<BulletPoolHandler>
    {
        private const string LOG_FORMAT = "<color=white><b>[BulletPoolManager]</b></color> {0}";

        public const string COMMON_BULLET = "CommonBullet";

        public static new BulletPoolManager Instance
        {
            get
            {
                return _instance as BulletPoolManager;
            }
            protected set
            {
                _instance = value;
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

            for (int i = 0; i < _InitInfos.Length; i++)
            {
                GameObject newObj = new GameObject(_InitInfos[i].Title + "PoolHandler");
                newObj.transform.SetParent(this.transform);

                BulletPoolHandler newHandler = newObj.AddComponent<BulletPoolHandler>();
                newHandler.Initialize();
                newHandler.ThisInitIndex = i;

                PoolHandlerDictionary.Add(_InitInfos[i].Title, newHandler);
            }
        }
    }

    public class BulletPoolHandler : BaseObjectPoolHandler
    {
        private const string LOG_FORMAT = "<color=white><b>[BulletPoolHandler]</b></color> {0}";

        public override void Initialize()
        {
            enableObjectsParent = new GameObject("Enables");
            enableObjectsParent.transform.SetParent(this.transform);

            disableObjectsParent = new GameObject("Disables");
            disableObjectsParent.transform.SetParent(this.transform);

            for (int i = 0; i < BulletPoolManager.Instance._InitInfos[ThisInitIndex].ObjInitCount; i++)
            {
                poolingQueue.Enqueue(CreateNewObject());
            }
        }

        protected override GameObject CreateNewObject()
        {
            GameObject newObj = Instantiate(BulletPoolManager.Instance._InitInfos[ThisInitIndex].Obj);
            newObj.gameObject.SetActive(false);
            newObj.transform.SetParent(disableObjectsParent.transform);
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
                newObj.gameObject.SetActive(true);
                newObj.transform.SetParent(enableObjectsParent.transform);
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

                newObj.gameObject.SetActive(true);
                newObj.transform.SetParent(enableObjectsParent.transform);
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

                newObj.gameObject.SetActive(true);
                newObj.transform.SetParent(enableObjectsParent.transform);
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