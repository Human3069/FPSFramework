using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VTSFramework.TSModule
{
    [RequireComponent(typeof(Button))]
    public class UI_InMissionPopupButton : MonoBehaviour
    {
        [SerializeField]
        protected Button button;
        [SerializeField]
        protected TextMeshProUGUI buttonText;

        protected GameObject _popupObj;

        public virtual void Initialize(string bottombarDirect, Vector2 buttonSize, UI_PopupEventReceiver popupEventReceiver, GameObject popupObj)
        {
            Debug.Assert(button != null);
            Debug.Assert(buttonText != null);

            this._popupObj = popupObj;

            Match patternMatch = Regex.Match(bottombarDirect, @"button:(\d+)");
            Match valueMatch = Regex.Match(patternMatch.Value, @"(\d+)");
            int localizeId = int.Parse(valueMatch.Value);

            string methodeName = Regex.Replace(bottombarDirect, @"button:(\d+)=>", "");
            button.onClick.AddListener(delegate { popupEventReceiver.SendMessage(methodeName); });
            button.onClick.AddListener(OnClickButton);

            buttonText.text = VTSManager.Instance.LocalizeTable.GetText(LocalizeTableReadHandler.SheetType.Popup, localizeId);

            RectTransform rectT = this.transform as RectTransform;
            rectT.sizeDelta = buttonSize;
        }

        protected virtual void OnClickButton()
        {
            _popupObj.SetActive(false);
        }
    }
}