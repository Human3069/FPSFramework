using Cinemachine;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace FPS_Framework
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class FPSAimHandler : MonoBehaviour
    {
        protected CinemachineVirtualCamera vCam;

        [SerializeField]
        protected float defaultFOV = 60f;
        [SerializeField]
        protected float aimedFOV = 40f;

        [Space(10)]
        [SerializeField]
        protected float fovDelta = 1f;

        protected virtual void Awake()
        {
            vCam = this.GetComponent<CinemachineVirtualCamera>();
        }

        public virtual void OnAimed(bool isAimed)
        {
            UniTaskEx.Cancel(this, 0);
            OnAimedAsync(isAimed).Forget();
        }

        protected virtual async UniTask OnAimedAsync(bool isAimed)
        {
            float targetFOV = isAimed ? aimedFOV : defaultFOV;
            while (vCam.m_Lens.FieldOfView != targetFOV)
            {
                vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, targetFOV, Time.deltaTime * fovDelta);

                await UniTaskEx.NextFrame(this, 0);
            }
        }
    }
}