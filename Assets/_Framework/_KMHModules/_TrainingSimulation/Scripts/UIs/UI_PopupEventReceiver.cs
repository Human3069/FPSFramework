using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    // 이 클래스는 전부 스트링 값으로 메소드를 호출합니다.
    public class UI_PopupEventReceiver : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_PopupEventReceiver]</b></color> {0}";

        private void OnClickVisionGuide_A_0_Popup_1_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_A_0_Popup_1_Button()");
        }

        private void OnClickVisionGuide_A_0_Popup_2_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_A_0_Popup_2_Button()");
        }

        private void OnClickVisionGuide_A_1_Popup_1_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_A_1_Popup_1_Button()");
        }

        private void OnClickVisionGuide_A_1_Popup_2_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_A_1_Popup_2_Button()");
        }

        private void OnClickVisionGuide_B_Popup_1_Button()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickVisionGuide_B_Popup_1_Button()");
        }
    }
}