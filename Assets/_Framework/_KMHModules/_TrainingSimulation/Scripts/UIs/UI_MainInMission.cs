using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VTSFramework.TSModule
{
    public class UI_MainInMission : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[UI_MainInMission]</b></color> {0}";

        [SerializeField]
        protected Button nextButton;
        [SerializeField]
        protected Button prevButton;
        [SerializeField]
        protected Button skipButton;

        protected _2DHighlight nextButtonHighlight;

        [Space(10)]
        [SerializeField]
        protected TextMeshProUGUI subtitleText;

        protected virtual void Awake()
        {
            InMissionHandler.OnIsNextButtonClickableValueChanged += OnNextButtonClickableValueChanged;
            InMissionHandler.OnTaskChanged += OnTaskChanged;

            nextButton.interactable = false;
            nextButton.onClick.AddListener(OnClickNextButton);
            nextButtonHighlight = nextButton.GetComponent<_2DHighlight>();

            prevButton.onClick.AddListener(OnClickPrevButton);

            skipButton.onClick.AddListener(OnClickSkipButton);
        }

        protected virtual void OnDestroy()
        {
            InMissionHandler.OnTaskChanged -= OnTaskChanged;
            InMissionHandler.OnIsNextButtonClickableValueChanged -= OnNextButtonClickableValueChanged;
        }

        public virtual void Initialize()
        {
            TaskRow targetTaskRow = VTSManager.Instance.ScenarioTable.FirstSheet.sortedTableList[0];
            subtitleText.text = VTSManager.Instance.LocalizeTable.GetText(LocalizeTableReadHandler.SheetType.InMission, targetTaskRow.Localize_Table_Id);
        }

        protected virtual void OnNextButtonClickableValueChanged(bool value)
        {
            nextButton.interactable = value;
            if (value == true)
            {
                nextButtonHighlight.enabled = true;
            }
            else
            {
                nextButtonHighlight.DisableAsync().Forget();
            }
        }

        protected virtual void OnTaskChanged(List<TaskRow> taskRowList)
        {
            TaskRow targetTaskRow = taskRowList[0];
            subtitleText.text = VTSManager.Instance.LocalizeTable.GetText(LocalizeTableReadHandler.SheetType.InMission, targetTaskRow.Localize_Table_Id);
        }

        protected virtual void OnClickNextButton()
        {
            InMissionHandler.Instance.RunNextStep();
        }

        protected virtual void OnClickPrevButton()
        {
            InMissionHandler.Instance.RunPrev();
        }

        protected virtual void OnClickSkipButton()
        {
            InMissionHandler.Instance.Skip();
        }
    }
}