using System;
using UnityEngine;


namespace _KMH_Framework._Internal_KeyCode
{
    public enum EventType
    {
        Click_Down = 0,
        Toggle_Down = 1,
    }

    [Serializable]
    public class KeyCodeData
    {
        [SerializeField]
        private KeyCode keyCode = KeyCode.None;
        public string KeyCodeName
        {
            get
            {
                return keyCode.ToString();
            }
        }

        [SerializeField]
        private EventType eventType = EventType.Click_Down;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        private bool _isInput = false;
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

                    if (eventType == EventType.Click_Down)
                    {
                        OnClick?.Invoke(value);
                    }
                    else if (eventType == EventType.Toggle_Down)
                    {
                        if (value == true)
                        {
                            toggleValue = !toggleValue;
                            OnValueChanged?.Invoke(toggleValue);
                        }
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }
        }

        internal bool IsInputDown
        {
            get
            {
                return Input.GetKeyDown(keyCode);
            }
        }

        [ReadOnly]
        [SerializeField]
        private bool toggleValue = false;

        private Action<bool> OnClick;
        private Action<bool> OnValueChanged;

        internal void OnUpdate()
        {
            IsInput = Input.GetKey(keyCode);
        }

        internal void RegisterEvent(Action<bool> action)
        {
            if (eventType == EventType.Click_Down)
            {
                OnClick += action;
            }
            else if (eventType == EventType.Toggle_Down)
            {
                OnValueChanged += action;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        internal void UnregisterEvent(Action<bool> action)
        {
            if (eventType == EventType.Click_Down)
            {
                OnClick -= action;
            }
            else if (eventType == EventType.Toggle_Down)
            {
                OnValueChanged -= action;
            }
            else
            {
                Debug.Assert(false);
            }
        }
    }
}