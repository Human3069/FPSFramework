using Kinemation.FPSFramework.Runtime.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace FPS_Framework
{
    public class MagnifableSight : BaseSight
    {
        private const string RETICLE_KEY = "Vector1_0bb2c494708d4e73aed6ec3922b741ac";

        [Space(10)]
        [SerializeField]
        protected Camera renderingCamera;
        [SerializeField]
        protected MeshRenderer lensRenderer;
        protected Material instantiatedMat;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float _magnifyRatio = 1f;
        public float MagnifyRatio
        {
            get
            {
                return _magnifyRatio; // 1, 0 -> 4, 1
            }
            protected set
            {
                if (_magnifyRatio != value)
                {
                    _magnifyRatio = Mathf.Clamp(value, 1f, maxMagnify);
                    renderingCamera.fieldOfView = -(4.4f * _magnifyRatio) + 39.5f;

                    float magnifyThreshold = (_magnifyRatio - 1) * (1 / (maxMagnify - 1));
                    float reticleSize = Mathf.Lerp(0.05f, 0.005f, magnifyThreshold);
                    // Debug.Log("magnifyThreshold : " + magnifyThreshold);
                    instantiatedMat.SetFloat(RETICLE_KEY, reticleSize); // 0.02 ~ 0.1
                }
            }
        }
        [SerializeField]
        protected float maxMagnify;

        protected override IEnumerator PostIsEquippedValueChangedRoutine()
        {
            while (IsEquipped == true)
            {
                MagnifyRatio += (Input.mouseScrollDelta.y * 0.1f);

                yield return null;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            Debug.Assert(renderingCamera != null);
            Debug.Assert(lensRenderer != null);
            Debug.Assert(MagnifyRatio >= 1f);
            Debug.Assert(MagnifyRatio <= maxMagnify);
        }

        protected void Start()
        {
            instantiatedMat = lensRenderer.material;

            renderingCamera.fieldOfView = 35f / _magnifyRatio;
        }
    }
}