using UnityEngine;

namespace VTSFramework.TSModule
{
    public class UI_PopupsPanel : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_PopupsPanel]</b></color> {0}";

        protected static UI_PopupsPanel _instance;
        public static UI_PopupsPanel Instance
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

        [HideInInspector]
        public UI_PopupEventReceiver popupEventReceiver;

        [SerializeField]
        protected Transform popupParentT;

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            popupEventReceiver = this.GetComponent<UI_PopupEventReceiver>();
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public virtual void InstantiatePopupConfig(UI_InMissionPopup inMissionPopupConfig, PopupRow row)
        {
            UI_InMissionPopup popupInstance = Instantiate(inMissionPopupConfig);
            popupInstance.transform.SetParent(popupParentT);
            popupInstance.Initialize(row, popupEventReceiver);
        }
    }
}