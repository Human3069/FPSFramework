using Cysharp.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace _KMH_Framework
{
    public class ConfigurationReader : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[ConfigurationReader]</b></color> {0}";

        protected static ConfigurationReader _instance;
        public static ConfigurationReader Instance
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

        public ConfigurationHandler<UserConfig> UserConfigHandler;
        public ConfigurationHandler<TCPClientConfig> TCPClientConfigHandler;
        public ConfigurationHandler<DatabaseConfig> DatabaseConfigHandler;

        protected bool isXMLReadDone = false;

        public async UniTask WaitUntilReady()
        {
            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return isXMLReadDone == true;
            }
        }

        protected void Awake()
        {
            Debug.Assert(isXMLReadDone == false);

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

            UserConfigHandler.Read();
            TCPClientConfigHandler.Read();
            DatabaseConfigHandler.Read();

            isXMLReadDone = true;
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