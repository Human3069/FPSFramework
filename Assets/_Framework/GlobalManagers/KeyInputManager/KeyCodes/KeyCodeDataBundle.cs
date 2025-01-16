using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _KMH_Framework._Internal_KeyCode
{
    public class KeyCodeDataBundle : ScriptableObject
    {
        [SerializeField]
        [SerializedDictionary("KeyType", "KeyCodeData")]
        internal SerializedDictionary<KeyType, KeyCodeData> KeySettingDic = new SerializedDictionary<KeyType, KeyCodeData>();
    }
}