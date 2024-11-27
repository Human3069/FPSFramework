using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class UIDManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UIDManager]</b></color> {0}";

        protected static UIDManager _instance;
        public static UIDManager Instance
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

        protected Dictionary<string, UIDObject> uidDic = new Dictionary<string, UIDObject>();

        [Header("Parents")]
        [SerializeField]
        protected Transform _2dUidParent;
        [SerializeField]
        protected Transform _3dUidParent;
        [SerializeField]
        protected Transform _utillUidParent;

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
        }

        public virtual async UniTask RegisterAllUidObjects()
        {
            await UniTask.RunOnThreadPool(Register2DUidObjects);
            await UniTask.RunOnThreadPool(Register3DUidObjects);
            await UniTask.RunOnThreadPool(RegisterUtillUidObjects);
        }

        protected virtual async UniTask Register2DUidObjects()
        {
            await UniTask.SwitchToMainThread();

            UIDObject[] _2dUidObjs = _2dUidParent.GetComponentsInChildren<UIDObject>();
            for (int i = 0; i < _2dUidObjs.Length; i++)
            {
                _2dUidObjs[i].Registered();
            }

            await UniTask.SwitchToThreadPool();

            for (int i = 0; i < _2dUidObjs.Length; i++)
            {
                _2dUidObjs[i]._UidType = UIDObject.UidType._2D;

                string correctUid = _2dUidObjs[i].UidValue.ToInMissionText();
                uidDic.Add(correctUid, _2dUidObjs[i]);
            }
        }

        protected virtual async UniTask Register3DUidObjects()
        {
            await UniTask.SwitchToMainThread();

            UIDObject[] _3dUidObjs = _3dUidParent.GetComponentsInChildren<UIDObject>();
            for (int i = 0; i < _3dUidObjs.Length; i++)
            {
                _3dUidObjs[i].Registered();
            }

            await UniTask.SwitchToThreadPool();

            for (int i = 0; i < _3dUidObjs.Length; i++)
            {
                _3dUidObjs[i]._UidType = UIDObject.UidType._3D;

                string correctUid = _3dUidObjs[i].UidValue.ToInMissionText();
                uidDic.Add(correctUid, _3dUidObjs[i]);
            }
        }

        protected virtual async UniTask RegisterUtillUidObjects()
        {
            await UniTask.SwitchToMainThread();

            UIDObject[] _utillUidObjs = _utillUidParent.GetComponentsInChildren<UIDObject>();
            for (int i = 0; i < _utillUidObjs.Length; i++)
            {
                _utillUidObjs[i].Registered();
            }

            await UniTask.SwitchToThreadPool();

            for (int i = 0; i < _utillUidObjs.Length; i++)
            {
                _utillUidObjs[i]._UidType = UIDObject.UidType._Utill;

                string correctUid = _utillUidObjs[i].UidValue.ToInMissionText();
                uidDic.Add(correctUid, _utillUidObjs[i]);
            }
        }

        public virtual UIDObject GetUIDObject(string uid)
        {
            if (uidDic.ContainsKey(uid) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "해당 uid [" + uid + "] 는 존재하지 않습니다.");
                return null;
            }
            else
            {
                return uidDic[uid];
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
    }
}