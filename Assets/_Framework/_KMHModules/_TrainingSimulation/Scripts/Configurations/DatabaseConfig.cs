using UnityEngine;

namespace _KMH_Framework
{
    [System.Serializable]
    public struct DatabaseConfig : IConfig<DatabaseConfig>
    {
        [ReadOnly]
        public string IPAddress;
        [ReadOnly]
        public string UserID;
        [ReadOnly]
        public string UserPassword;
        [ReadOnly]
        public string CharSet;

        [Space(10)]
        [ReadOnly]
        public string DatabaseName;

        public DatabaseConfig Parsed()
        {
            return this;
        }
    }
}