using UnityEngine;

namespace _KMH_Framework._TS_Module
{
    /// <summary>
    /// Hierachy에서 생성된 MonoSingleton(GameObject)면 Awake()를 사용 <para/>
    /// T.Instance로 생성된 MonoSingleton이면 Initialized()를 사용 <para/>
    /// </summary>
    abstract public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance = null;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType(typeof(T)) as T;
                    Instance = _instance ?? (new GameObject(typeof(T).Name)).AddComponent<T>();
                }
                return _instance;
            }

            private set
            {
                if (value != null)
                {
                    _instance = value;
                }
            }            
        }
        abstract public void Awake();
    }
}