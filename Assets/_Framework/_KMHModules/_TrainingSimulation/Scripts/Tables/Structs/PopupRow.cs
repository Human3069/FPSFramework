using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public struct PopupRow : IRow<PopupRow>
    {
        public PopupRow Validated()
        {
            Popup_Uid = Popup_Uid.ToInMissionText();
            Popup_Config = Popup_Config.ToInMissionText();
            Popup_Size = Popup_Size.ToInMissionText();
            Popup_Pos = Popup_Pos.ToInMissionText();

            Toolbar_Direct = Toolbar_Direct.ToInMissionText();
            Content_Direct = Content_Direct.ToInMissionText();

            // Bottombar_Direct = Bottombar_Direct.ToInMissionText();
            if (string.IsNullOrEmpty(Bottombar_Direct) == false)
            {
                Bottombar_Direct = Bottombar_Direct.Replace(" ", "").Replace("\n", "").Trim();
            }

            return this;
        }

        [ReadOnly]
        public string Popup_Uid;
        [ReadOnly]
        public string Popup_Config;
        [ReadOnly]
        public string Popup_Size;
        [ReadOnly]
        public string Popup_Pos;

        [Space(10)]
        [ReadOnly]
        public string Toolbar_Direct;
        [ReadOnly]
        public string Content_Direct;
        [ReadOnly]
        public string Bottombar_Direct;
    }
}