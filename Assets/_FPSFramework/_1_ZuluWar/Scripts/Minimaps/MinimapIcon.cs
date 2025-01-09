using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace FPS_Framework
{
    public class MinimapIcon : MonoBehaviour
    {
        [SerializeField]
        protected SpriteRenderer spriteRenderer;

        [Space(10)]
        [SerializeField]
        protected bool isRotationFixed = true;

        protected void Awake()
        {
            AwakeAsync().Forget();
        }

        protected async UniTaskVoid AwakeAsync()
        {
            await UniTask.WaitUntil(() => LayerManager.Instance != null);

            Debug.Assert(spriteRenderer.gameObject.layer == LayerManager.Instance.MinimapLayer);

            if (isRotationFixed == false)
            {
                CancellationToken tokenOnDestroy = this.GetCancellationTokenOnDestroy();
                while (true)
                {
                    await UniTask.Yield(tokenOnDestroy);
                }
            }
        }
    }
}