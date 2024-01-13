using System.Collections;
using TMPro;
using UnityEngine;

namespace _KMH_Framework
{
    public class UI_EnteredKey : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_EnteredKey]</b></color> {0}";

        protected static int enteredKeyCount = 0;
        protected int id;

        protected KeyInputManager _manager;

        [SerializeField]
        public TMP_Text contentText;
        [SerializeField]
        public TMP_Text buttonText;

        public bool isPressed;

        protected virtual void Awake()
        {
            id = enteredKeyCount;
            enteredKeyCount++;

            StartCoroutine(PostAwake());
        }

        protected virtual IEnumerator PostAwake()
        {
            while (KeyInputManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "yield return null");

                yield return null;
            }
            _manager = KeyInputManager.Instance;

            contentText.text = _manager.KeySettings[id].name;
            buttonText.text = _manager.KeySettings[id].keyCode.ToString();

            _manager.OnSettingStateChanged += OnSettingChanged;
        }

        protected virtual void OnDestroy()
        {
            _manager.OnSettingStateChanged -= OnSettingChanged;
        }

        public virtual void OnClickBehaviourOutputButtonDown()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<color=white><b>BehaviourOutput</b></color>Button(), id : " + id);
            Debug.Assert(_manager._SettingState == KeyInputManager.SettingState.None);

            _manager.OnClickEnteredKeyButton(id); // forcelly call!

            Debug.Assert(_manager._SettingState == KeyInputManager.SettingState.WaitForInput);

            buttonText.text = null;
        }

        protected virtual void OnSettingChanged(KeyInputManager.SettingState _state, KeyCode _keyCode, int index)
        {
            if (index == id)
            {
                Debug.LogFormat(LOG_FORMAT, "On<color=white><b>Setting</b></color>Changed(), _state : <color=green><b>" + _state + "</b></color>, _keyCode : <color=green><b>" + _keyCode + "</b></color>, index : <color=green><b>" + index + "</b></color>");

                if (_state == KeyInputManager.SettingState.None)
                {
                    _manager.KeySettings[id].keyCode = _keyCode;
                    buttonText.text = _keyCode.ToString();
                }
                else if (_state == KeyInputManager.SettingState.WaitForInput)
                {

                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }
    }
}