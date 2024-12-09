using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    [System.Serializable]
    public class KeySetting
    {
        [SerializeField]
        internal string Name;
        [SerializeField]
        internal KeyCode _KeyCode;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        internal bool _isInput;
        internal bool IsInput
        {
            get
            {
                return _isInput;
            }
            set
            {
                if (_isInput != value)
                {
                    _isInput = value;

                    Invoke_OnValueChanged(value);
                }
            }
        }

        internal bool IsInputDown
        {
            get
            {
                return Input.GetKeyDown(_KeyCode);
            }
        }

        public delegate void ValueChanged(bool _value);
        public event ValueChanged OnValueChanged;

        protected internal void Invoke_OnValueChanged(bool _value)
        {
            if (OnValueChanged != null)
            {
                OnValueChanged(_value);
            }
        }
    }

    public class KeyInputManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[KeyInputManager]</b></color> {0}";

        protected static KeyInputManager _instance;
        public static KeyInputManager Instance
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
        protected KeySetting[] _keySettings;
        public KeySetting[] _KeySettings
        {
            get
            {
                return _keySettings;
            }
        }

        public Dictionary<string, KeySetting> KeyData = new Dictionary<string, KeySetting>();

        public bool HasInput(string name)
        {
            if (KeyData.ContainsKey(name) == false)
            {
                throw new System.NotSupportedException("has no key : " + name);
            }

            return KeyData[name].IsInput;
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<color=white><b>Instance Overlapped</b></color> While On Awake()");
                Destroy(this);
                return;
            }

            for (int i = 0; i < _KeySettings.Length; i++)
            {
                KeyData.Add(_KeySettings[i].Name, _KeySettings[i]);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void Update()
        {
            foreach(KeySetting key in _KeySettings)
            {
                key.IsInput = Input.GetKey(key._KeyCode);
            }
        }

#if false
        protected virtual void OnGUI()
        {
            // this Calls Once While Get KeyCode
            if (_SettingState == SettingState.WaitForInput)
            {
                Event _event = Event.current;
                if (_event.keyCode != KeyCode.None)
                {
                    Debug.LogFormat(LOG_FORMAT, "Input Key : <color=green><b>" + _event.keyCode + "</b></color>");

                    keyCodeParam = _event.keyCode;
                    _SettingState = SettingState.None;
                }
            }
        }
#endif
    }
}