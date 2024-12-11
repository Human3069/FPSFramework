using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public class BulletPoolManager : BaseObjectPoolManager<BulletPoolHandler>
    {
        private const string LOG_FORMAT = "<color=white><b>[BulletPoolManager]</b></color> {0}";

        public static string _556_AR_BULLET;
        public static string _577_450_SR_BULLET;
        public static string _762_AR_BULLET;
        public static string _762_SR_BULLET;
        public static string _9_SMG_BULLET;

        public static string _SHRAPNEL_PIECE;

        public static string _57_SHRAPNEL_BULLET;
        public static string _40_CANNON_BULLET;

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

            _556_AR_BULLET = _InitInfos[0].Obj.name;
            _577_450_SR_BULLET = _InitInfos[1].Obj.name;
            _762_AR_BULLET = _InitInfos[2].Obj.name;
            _762_SR_BULLET = _InitInfos[3].Obj.name;
            _9_SMG_BULLET = _InitInfos[4].Obj.name;

            _SHRAPNEL_PIECE = _InitInfos[5].Obj.name;

            _57_SHRAPNEL_BULLET = _InitInfos[6].Obj.name;
            _40_CANNON_BULLET = _InitInfos[7].Obj.name;

            for (int i = 0; i < _InitInfos.Length; i++)
            {
                string objName = _InitInfos[i].Obj.name;
                GameObject newObj = new GameObject(objName + "PoolHandler");
                newObj.transform.SetParent(this.transform);

                BulletPoolHandler newHandler = newObj.AddComponent<BulletPoolHandler>();
                newHandler.ThisInitIndex = i;
                newHandler.Initialize();

                PoolHandlerDictionary.Add(objName, newHandler);
            }

            IsReady = true;
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