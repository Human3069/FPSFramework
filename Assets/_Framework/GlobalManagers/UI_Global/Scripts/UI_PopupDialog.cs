using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _KMH_Framework
{
    public class UI_PopupDialog : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_PopupDialog]</b></color> {0}";

        protected static UI_PopupDialog _instance;
        public static UI_PopupDialog Instance
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

        [Header("PopupDialog Panel")]
        [SerializeField]
        protected GameObject popupDialogPanel;
        [SerializeField]
        protected GameObject popupDialogContentPanel;

        [Header("Title Panel")]
        [SerializeField]
        protected GameObject titlePanel;

        [Header("Texts")]
        [SerializeField]
        protected TMP_Text titleText;
        [SerializeField]
        protected TMP_Text contentText;

        [Space(10)]
        [SerializeField]
        protected TMP_Text yesButtonText;
        [SerializeField]
        protected TMP_Text noButtonText;

        [Header("Buttons")]
        [SerializeField]
        protected Button yesButton;
        [SerializeField]
        protected Button noButton;

        public delegate void ClickPopupDialogButtonResult(string buttonText);
        public ClickPopupDialogButtonResult OnClickPopupDialogButtonResult;

        protected virtual void Awake()
        {
            Debug.Assert(popupDialogPanel.activeSelf == false);
            Debug.Assert(popupDialogContentPanel.activeSelf == true);

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "Instance Overlapped While <b>Awake()</b>");
                Destroy(this);
                return;
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

        public void Show(string content, ClickPopupDialogButtonResult _popupDialogResult = null)
        {
            this.yesButtonText.text = "Yes";
            this.noButtonText.text = "No";

            Show(content, "", 2, _popupDialogResult);
        }

        public void Show(string content, string _yesButtonText, string _noButtonText, ClickPopupDialogButtonResult _popupDialogResult = null)
        {
            this.yesButtonText.text = _yesButtonText;
            this.noButtonText.text = _noButtonText;

            Show(content, "", 2, _popupDialogResult);
        }

        public void Show(string content, string _yesButtonText, string _noButtonText, int buttonCount = 2, ClickPopupDialogButtonResult _popupDialogResult = null)
        {
            this.yesButtonText.text = _yesButtonText;
            this.noButtonText.text = _noButtonText;

            Show(content, "", buttonCount, _popupDialogResult);
        }

        public void Show(string content, int buttonCount = 2, ClickPopupDialogButtonResult _popupDialogResult = null)
        {
            Show(content, "", buttonCount, _popupDialogResult);
        }

        public void Show(string content, string title = "", ClickPopupDialogButtonResult _popupDialogResult = null)
        {
            Show(content, title, 2, _popupDialogResult);
        }

        public void Show(string content, string title = "", int buttonCount = 2, ClickPopupDialogButtonResult _popupDialogResult = null)
        {
            Debug.LogFormat(LOG_FORMAT, "PopupDialog.Show()");

            if (string.IsNullOrEmpty(title) == true)
            {
                titlePanel.SetActive(false);
            }
            else
            {
                titleText.text = title;
                titlePanel.SetActive(true);
            }

            contentText.text = content;

            if (buttonCount == 1)
            {
                yesButton.gameObject.SetActive(true);
                noButton.gameObject.SetActive(false);
            }
            else if (buttonCount == 2)
            {
                yesButton.gameObject.SetActive(true);
                noButton.gameObject.SetActive(true);
            }
            else
            {
                Debug.Assert(false);
            }

            OnClickPopupDialogButtonResult = _popupDialogResult;
          
            popupDialogPanel.SetActive(true);
            popupDialogContentPanel.SetActive(true);
        }

        public virtual void OnClickPopupDialogButton(string buttonText)
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<b>PopupDialog</b>Button(), buttonText : <color=yellow>" + buttonText + "</color>");

            if (OnClickPopupDialogButtonResult != null)
            {
                OnClickPopupDialogButtonResult(buttonText);
            }
            OnClickPopupDialogButtonResult = null;
        }
    }
}