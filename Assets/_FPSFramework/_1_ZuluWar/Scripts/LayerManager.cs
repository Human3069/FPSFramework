using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    public class LayerManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[LayerManager]</b></color> {0}";

        protected static LayerManager _instance;
        public static LayerManager Instance
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
        protected LayerMask _defaultLayerMask;
        public int DefaultLayer
        {
            get
            {
                return (int)Mathf.Log(_defaultLayerMask.value, 2);
            }
        }

        [SerializeField]
        protected LayerMask _minimapLayerMask;
        public int MinimapLayer
        {
            get
            {
                return (int)Mathf.Log(_minimapLayerMask.value, 2);
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
                Debug.LogErrorFormat(LOG_FORMAT, "Instance already exists. Destroying this instance.");
                Destroy(this.gameObject);
                return;
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