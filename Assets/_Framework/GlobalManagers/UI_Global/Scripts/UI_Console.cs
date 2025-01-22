using FPS_Framework;
using FPS_Framework.ZuluWar;
using TMPro;
using UnityEngine;

namespace _KMH_Framework
{
    public class UI_Console : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_Console]</b></color> {0}";

        [Header("Panels")]
        [SerializeField]
        protected GameObject consolePanel;

        [Header("Input Field")]
        [SerializeField]
        protected TMP_InputField inputField;

        protected virtual void Awake()
        {
            Debug.Assert(consolePanel.activeSelf == false);
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown("`") == true)
            {
                Debug.LogFormat(LOG_FORMAT, "React Developer Console!");

                consolePanel.SetActive(!consolePanel.activeSelf);
                if (consolePanel.activeSelf == true)
                {
                    Time.timeScale = 0;
                    FPSControllerEx.Instance.UpdateMouseShowState(true);

                    inputField.ActivateInputField();
                    inputField.Select();
                }
                else
                {
                    Time.timeScale = 1;
                    FPSControllerEx.Instance.UpdateMouseShowState(false);
                }
            }

            if (consolePanel.activeSelf == true)
            {
                if (Input.GetKeyDown(KeyCode.Return) == true)
                {
                    string consoleInput = inputField.text;
                    inputField.text = null;

                    Debug.LogFormat(LOG_FORMAT, "inputField : " + consoleInput);
                    if (consoleInput.StartsWith("give money ") == true)
                    {
                        if (int.TryParse(consoleInput.Replace("give money ", ""), out int gaveMoney) == true)
                        {
                            GameManager.Instance._PhaseCounter.TotalMoney += gaveMoney;
                        }
                    }
                }
            }
        }
    }
}