using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    [System.Serializable]
    public class KeySetting
    {
        [SerializeField]
        internal string name;
        [SerializeField]
        internal KeyCode keyCode;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        internal bool _isInput;
        internal bool isInput
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

        internal bool _isInputDown;
        internal bool IsInputDown
        {
            get
            {
                return _isInputDown;
            }
            set
            {
                if (_isInputDown != value)
                {
                    _isInputDown = value;
                }
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

        public static string KEY_MOVE_FORWARD;
        public static string KEY_MOVE_BACKWARD;
        public static string KEY_MOVE_LEFT;
        public static string KEY_MOVE_RIGHT;
        public static string KEY_SPRINT;
        public static string KEY_JUMP;
        public static string KEY_CROUCH;
        public static string KEY_PRONE;
        public static string KEY_LEAN_LEFT;
        public static string KEY_LEAN_RIGHT;
        public static string KEY_RELOAD;
        public static string KEY_INTERACT;
        public static string KEY_THROW_GRENADE;

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

        public KeySetting[] KeySettings;

        public enum SettingState
        {
            None,
            WaitForInput
        }

        [ReadOnly]
        [SerializeField]
        protected SettingState _settingState = SettingState.None;
        public SettingState _SettingState
        {
            get
            {
                return _settingState;
            }
            set
            {
                _settingState = value;

                Invoke_OnSettingStateChanged();
            }
        }

        public delegate void SettingStateChanged(SettingState _value, KeyCode _keyCode, int index);
        public event SettingStateChanged OnSettingStateChanged;

        protected virtual void Invoke_OnSettingStateChanged()
        {
            if (OnSettingStateChanged != null)
            {
                OnSettingStateChanged(_SettingState, keyCodeParam, ui_Id);
            }
        }

        protected int ui_Id = -1;
        protected KeyCode keyCodeParam = KeyCode.None;

        protected bool _isInitialized = false;
        public bool IsInitialized
        {
            get
            {
                return _isInitialized;
            }
            protected set
            {
                _isInitialized = value;
            }
        }

        public Dictionary<string, KeySetting> KeyData = new Dictionary<string, KeySetting>();

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

            for (int i = 0; i < KeySettings.Length; i++)
            {
                KeyData.Add(KeySettings[i].name, KeySettings[i]);
            }

            KEY_MOVE_FORWARD = KeySettings[0].name;
            KEY_MOVE_BACKWARD = KeySettings[1].name;
            KEY_MOVE_LEFT = KeySettings[2].name;
            KEY_MOVE_RIGHT = KeySettings[3].name;
            KEY_SPRINT = KeySettings[4].name;
            KEY_JUMP = KeySettings[5].name;
            KEY_CROUCH = KeySettings[6].name;
            KEY_PRONE = KeySettings[7].name;
            KEY_LEAN_LEFT = KeySettings[8].name;
            KEY_LEAN_RIGHT = KeySettings[9].name;
            KEY_RELOAD = KeySettings[10].name;
            KEY_INTERACT = KeySettings[11].name;
            KEY_THROW_GRENADE = KeySettings[12].name;

            IsInitialized = true;
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
            for (int i = 0; i < KeySettings.Length; i++)
            {
                if (_SettingState == SettingState.None)
                {
                    KeySettings[i].IsInputDown = Input.GetKeyDown(KeySettings[i].keyCode);
                    KeySettings[i].isInput = Input.GetKey(KeySettings[i].keyCode);
                }
            }
        }

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


        public virtual void OnClickEnteredKeyButton(int id)
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<color=white><b>EnteredKey</b></color>Button(), id : " + id);

            if (_SettingState == SettingState.None)
            {
                ui_Id = id;
                _SettingState = SettingState.WaitForInput;
            }
            else
            {
                //
            }
        }
    }
}