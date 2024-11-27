using UnityEngine;

namespace VTSFramework.TSModule
{
    public class GlobalObjectUtilities : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[GlobalObjectUtilities]</b></color> {0}";

        protected static GlobalObjectUtilities _instance;
        public static GlobalObjectUtilities Instance
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

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            DontDestroyOnLoad(this.gameObject);
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