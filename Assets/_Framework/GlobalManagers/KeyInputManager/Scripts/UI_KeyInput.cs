using Cysharp.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;

namespace _KMH_Framework
{
    public class UI_KeyInput : MonoBehaviour
    {
#if false
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
        [SerializeField]
        protected GameObject settingChildPanel;

        public bool IsSettingPanelOpened = false;

        protected Vector3 settingChildPanelPos;

        [Header("Senstivities")]
        [SerializeField]
        protected TextMeshProUGUI droneModeSenstivityValueText;
        [SerializeField]
        protected TextMeshProUGUI fpsModeSenstivityValueText;

        public delegate void DroneModeSenstivityChanged(float value);
        public static event DroneModeSenstivityChanged OnDroneModeSenstivityChanged;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        protected float _droneModeSenstivity = 1f;
        public float DroneModeSenstivity
        {
            get
            {
                return _droneModeSenstivity;
            }
            protected set
            {
                _droneModeSenstivity = value;

                if (OnDroneModeSenstivityChanged != null)
                {
                    OnDroneModeSenstivityChanged(value);
                }
            }
        }

        public delegate void FPSModeSenstivityChanged(float value);
        public static event FPSModeSenstivityChanged OnFPSModeSenstivityChanged;

        [ReadOnly]
        [SerializeField]
        protected float _fpsModeSenstivity = 1f;
        public float FpsModeSenstivity
        {
            get
            {
                return _fpsModeSenstivity;
            }
            protected set
            {
                _fpsModeSenstivity = value;

                if (OnFPSModeSenstivityChanged != null)
                {
                    OnFPSModeSenstivityChanged(value);
                }
            }
        }

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

            Debug.Assert(settingPanel.activeSelf == false);
            PostAwake().Forget();
        }

        protected virtual async UniTaskVoid PostAwake()
        {
            await UniTask.WaitWhile(PredicateFunc);
            bool PredicateFunc()
            {
                return KeyInputManager.Instance == null;
            }

            for (int i = 0; i < KeyInputManager.Instance._KeySettings.Length; i++)
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

        public virtual void ActiveSettingPanel()
        {
            Debug.LogFormat(LOG_FORMAT, "ActiveSettingPanel()");

            IsSettingPanelOpened = true;

            settingChildPanelPos = settingChildPanel.transform.position;
            settingPanel.SetActive(true);
            settingChildPanel.SetActive(true);
        }

        public virtual void OnClickCloseButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickCloseButton()");

            IsSettingPanelOpened = false;
        }

        protected void OnTweenedCloseButtonAction()
        {
            settingChildPanel.transform.position = settingChildPanelPos;
        }

        public void OnValueChangedDroneModeSenstivitySlider(float value)
        {
            droneModeSenstivityValueText.text = value.ToString("F2");
            DroneModeSenstivity = value;
        }

        public void OnValueChangedFPSModeSenstivitySlider(float value)
        {
            fpsModeSenstivityValueText.text = value.ToString("F2");
            FpsModeSenstivity = value;
        }
#endif
    }
}