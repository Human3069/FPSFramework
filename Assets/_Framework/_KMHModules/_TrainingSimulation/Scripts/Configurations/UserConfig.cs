using System.Xml.Serialization;
using UnityEngine;

namespace _KMH_Framework
{
    [System.Serializable]
    public struct UserConfig : IConfig<UserConfig>
    {
        [ReadOnly]
        public string UseCheat;
        [HideInInspector]
        public bool IsUseCheat;

        [ReadOnly]
        public string ClientVersion;

        [Space(10)]
        [ReadOnly]
        public float DelayBetweenStep;
        [ReadOnly]
        public float MaxInteractableDistance;
        [ReadOnly]
        public string RaycastAll;
        [HideInInspector]
        public bool IsRaycastAll;

        public UserConfig Parsed()
        {
            IsUseCheat = bool.Parse(UseCheat);
            IsRaycastAll = bool.Parse(RaycastAll);

            return this;
        }
    }
}