using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public class UI_KeyInput : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_KeyInput]</b></color> {0}";

        protected static UI_KeyInput _instance;
        public static UI_KeyInput Instance
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

        [Header("Instantiating UI")]
        [SerializeField]
        protected GameObject instantiate_UI_Obj;
        [SerializeField]
        protected Transform instantiateTransform;

        [Header("Global UI")]
        [SerializeField]
        protected GameObject settingPanel;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Instance Overlapped !");
                Destroy(this.gameObject);
                return;
            }

            StartCoroutine(PostAwake());

            Debug.Assert(settingPanel.activeSelf == false);
        }

        protected virtual IEnumerator PostAwake()
        {
            while (KeyInputManager.Instance == null)
            {
                Debug.LogFormat(LOG_FORMAT, "yield return null");

                yield return null;
            }

            for (int i = 0; i < KeyInputManager.Instance.KeySettings.Length; i++)
            {
                Instantiate(instantiate_UI_Obj, instantiateTransform);
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

        public virtual void SetSettingPanelEnable(bool value)
        {
            Debug.LogFormat(LOG_FORMAT, "Set<color=white><b>SettingPanel</b></color>Enable(), value : <color=green><b>" + value + "</b></color>");

            settingPanel.SetActive(value);
        }
    }
}