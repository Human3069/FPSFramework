using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [RequireComponent(typeof(UIDObject))]
    public abstract class BaseObject : MonoBehaviour
    {
        [HideInInspector]
        public UIDObject UidObj;

        public virtual void OnRegistered(UIDObject uidObj)
        {
            this.UidObj = uidObj;
        }

        public virtual void SetSetting(string order)
        {
            //
        }
    }
}