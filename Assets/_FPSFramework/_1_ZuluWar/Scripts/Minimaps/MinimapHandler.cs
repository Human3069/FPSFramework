using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    public class MinimapHandler : MonoBehaviour
    {
        [SerializeField]
        protected Camera minimapCamera;
        [SerializeField]
        protected Transform playerT;

        protected void Awake()
        {
            AwakeAsync().Forget();
        }

        protected async UniTaskVoid AwakeAsync()
        {
            await UniTask.WaitUntil(() => LayerManager.Instance != null);

            minimapCamera.cullingMask = LayerManager.Instance.MinimapCullingMask |
                                        LayerManager.Instance.DefaultCullingMask;
        }

        protected void FixedUpdate()
        {
            this.transform.position = playerT.position;
            this.transform.eulerAngles = new Vector3(0f, playerT.eulerAngles.y, 0f);
        }
    }
}