using TMPro;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [DisallowMultipleComponent]
    public class UI_InMissionPopup : BaseObject, ITypable
    {
        [SerializeField]
        protected Transform buttonParentT;

        [Space(10)]
        [SerializeField]
        protected TextMeshProUGUI[] toolbarTexts;
        [SerializeField]
        protected TextMeshProUGUI[] contentTexts;

        [Space(10)]
        [SerializeField]
        protected Vector2 buttonSize = new Vector2(140f, 40f);

        protected InMissionType _inMissionType;
        public InMissionType _InMissionType
        {
            get
            {
                return _inMissionType;
            }
            set
            {
                _inMissionType = value;
            }
        }

        public virtual void Initialize(PopupRow row, UI_PopupEventReceiver popupEventReceiver)
        {
            this.name = "Popup [" + row.Popup_Uid + "]";
            RectTransform rectT = this.transform as RectTransform;
            UIDObject uidObj = this.GetComponent<UIDObject>();
            Debug.Assert(uidObj != null);

            uidObj.UidValue = row.Popup_Uid;

            string[] sizeValues = row.Popup_Size.Split('x');
            float xValue = float.Parse(sizeValues[0]);
            float yValue = float.Parse(sizeValues[1]);
            rectT.sizeDelta = new Vector2(xValue, yValue);

            string[] posValues = row.Popup_Pos.Split("x");
            xValue = float.Parse(posValues[0]);
            yValue = float.Parse(posValues[1]);
            rectT.anchoredPosition = new Vector2(xValue, yValue);

            if (string.IsNullOrEmpty(row.Toolbar_Direct) == false)
            {
                string[] toolbarDirects = row.Toolbar_Direct.Split('&');
                for (int i = 0; i < toolbarDirects.Length; i++)
                {
                    string toolbarStringId = toolbarDirects[i].Replace("text:", "");
                    int localizeId = int.Parse(toolbarStringId);
                    toolbarTexts[i].text = VTSManager.Instance.LocalizeTable.GetText(LocalizeTableReadHandler.SheetType.Popup, localizeId);
                }
            }
           
            if (string.IsNullOrEmpty(row.Content_Direct) == false)
            {
                string[] contentDirects = row.Content_Direct.Split('&');
                for (int i = 0; i < contentDirects.Length; i++)
                {
                    if (contentDirects[i].Contains("text:") == true)
                    {
                        string contentStringId = contentDirects[i].Replace("text:", "");
                        int localizeId = int.Parse(contentStringId);
                        contentTexts[i].text = VTSManager.Instance.LocalizeTable.GetText(LocalizeTableReadHandler.SheetType.Popup, localizeId);
                    }
                }
            }

            UI_InMissionPopupButton buttonResource = Resources.Load<UI_InMissionPopupButton>("UI/Button_InMissionPopup");
            
            if (string.IsNullOrEmpty(row.Bottombar_Direct) == false)
            {
                string[] bottombarDirects = row.Bottombar_Direct.Split("&");
                foreach (string bottombarDirect in bottombarDirects)
                {
                    UI_InMissionPopupButton buttonInstance = Instantiate(buttonResource);
                    buttonInstance.Initialize(bottombarDirect, buttonSize, popupEventReceiver, this.gameObject);
                    buttonInstance.transform.SetParent(buttonParentT);
                }
            }
        }

        public override void OnRegistered(UIDObject uidObj)
        {
            base.OnRegistered(uidObj);

            this.gameObject.SetActive(false);
        }
    }
}