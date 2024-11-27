
using UnityEngine;

namespace _KMH_Framework
{
    [System.Serializable]
    public struct TCPClientConfig : IConfig<TCPClientConfig>
    {
        [ReadOnly]
        public string ServerIPAddress;
        [ReadOnly]
        public int ServerPortNumber;

        public TCPClientConfig Parsed()
        {
            return this;
        }
    }
}