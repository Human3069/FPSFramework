using _KMH_Framework;
using Cysharp.Threading.Tasks;
using FPS_Framework.Pool;
using System;
using System.Collections;
using UnityEngine;

namespace FPS_Framework
{
    public class ImpactHandler : MonoBehaviour
    {
        [SerializeField]
        protected ImpactType impactType;
        [SerializeField]
        protected float lifeTime = 20;
   
        protected void OnEnable()
        {
            OnEnableAsync().Forget();
        }

        protected async UniTaskVoid OnEnableAsync()
        {
            await UniTask.WaitForSeconds(lifeTime);

            this.gameObject.ReturnPool(impactType);
        }

#if UNITY_EDITOR
        [ContextMenu("Parse From Name")]
        protected void ParseFromName()
        {
            string name = this.gameObject.name;
            if (Enum.TryParse(name, out ImpactType type) == true)
            {
                impactType = type;
            }
            else
            {
                Debug.LogError("Cannot parse from name : " + name);
            }
        }
#endif
    }
}