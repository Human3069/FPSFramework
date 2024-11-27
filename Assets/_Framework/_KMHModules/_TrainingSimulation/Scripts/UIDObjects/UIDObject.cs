using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [DisallowMultipleComponent]
    public class UIDObject : MonoBehaviour
    {
        [HideInInspector]
        public BaseObject _BaseObj;

        [Header("=== UIDObject ===")]
        public string UidValue;

        public enum UidType
        {
            NonRegistered,

            _2D,
            _3D,
            _Utill
        }

        [ReadOnly]
        public UidType _UidType = UidType.NonRegistered;

        public virtual void Registered()
        {
            Debug.Assert(string.IsNullOrEmpty(UidValue) == false, "UID 값이 비어있습니다 [" + this.transform.parent.name + "/" + this.name + "]");

            _BaseObj = this.GetComponent<BaseObject>();
            if (_BaseObj != null)
            {
                _BaseObj.OnRegistered(this);
            }
        }
    }
}