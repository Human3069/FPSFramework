using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    public class FPSManager : MonoBehaviour
    {
        protected static FPSManager _instance;
        public static FPSManager Instance
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

        [SerializeField]
        protected float _mouseSenstivity = 1f;
        
        [ReadOnly]
        [SerializeField]
        protected float _aimThreshold = 1f;
        public float AimThreshold
        {
            get
            {
                return _aimThreshold;
            }
            set
            {
                _aimThreshold = value;
            }
        }

        public float ActualSenstivity
        {
            get
            {
                return _mouseSenstivity * AimThreshold;
            }
        }

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(_Log._Format(this), "Awake()");
            }
        }

        protected void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}