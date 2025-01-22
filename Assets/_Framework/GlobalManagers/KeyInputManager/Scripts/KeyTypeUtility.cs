using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Xml.Serialization;
using UnityEngine;

namespace _KMH_Framework
{
    public enum KeyType
    {
        Move_Forward,
        Move_Backward,
        Move_Right,
        Move_Left,

        Lean_Right,
        Lean_Left,

        Sprint,
        Jump,

        Crouch,
        Prone,

        Reload,
        Interact,
        Throw_Grenade,
        Change_FireMode,

        Toggle_Shop,
    }

    public static class KeyTypeUtility
    {
        public static bool IsInput(this KeyType type)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
                return false;
            }
            else
            {
                return KeyCodeManager.Instance.GetData(type).IsInput;
            }
        }

        public static bool IsInputDown(this KeyType type)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
                return false;
            }
            else
            {
                return KeyCodeManager.Instance.GetData(type).IsInputDown;
            }
        }

        public static bool ToggleValue(this KeyType type)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
                return false;
            }
            else
            {
                return KeyCodeManager.Instance.GetData(type).ToggleValue;
            }
        }

        public static void RegisterEvent(this KeyType type, Action<bool> action)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
            }
            else
            {
                KeyCodeManager.Instance.GetData(type).RegisterEvent(action);
            }
        }

        public static IEnumerator RegisterEventRoutine(this KeyType type, Action<bool> action)
        {
            yield return new WaitWhile(() => KeyCodeManager.Instance == null);

            KeyCodeManager.Instance.GetData(type).RegisterEvent(action);
        }

        public static async UniTask RegisterEventAsync(this KeyType type, Action<bool> action)
        {
            await UniTask.WaitWhile(() => KeyCodeManager.Instance == null);

            KeyCodeManager.Instance.GetData(type).RegisterEvent(action);
        }

        public static void UnregisterEvent(this KeyType type, Action<bool> action)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
            }
            else
            {
                KeyCodeManager.Instance.GetData(type).UnregisterEvent(action);
            }
        }

        public static void UpdateLock(this KeyType type, bool isLock)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
            }
            else
            {
                KeyCodeManager.Instance.GetData(type).UpdateLock(isLock);
            }
        }

        public static void SetToggleValue(this KeyType type, bool isOn)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
            }
            else
            {
                KeyCodeManager.Instance.GetData(type).SetToggleValue(isOn);
            }
        }

        public static string GetKeyName(this KeyType type)
        {
            if (KeyCodeManager.Instance == null)
            {
                Debug.Assert(false);
                return null;
            }
            else
            {
                return KeyCodeManager.Instance.GetData(type).KeyCodeName;
            }
        }
    }
}