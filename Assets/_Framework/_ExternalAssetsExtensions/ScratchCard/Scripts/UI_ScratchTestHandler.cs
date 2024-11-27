using ScratchCardAsset.Animation;
using ScratchCardAsset.Core;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace _KMH_Framework
{
	public class UI_ScratchTestHandler : MonoBehaviour
	{
		private const string LOG_FORMAT = "<color=white><b>[UI_ScratchTestHandler]</b></color> {0}";

        [SerializeField]
		private ScratchCardHandlerEx cardManager;

		[Header("Progress")]
		[SerializeField]
		private Text progressText;

		[Header("Brushes")]
        [SerializeField]
        private Texture brush_1_Texture;
        [SerializeField]
        private Texture brush_2_Texture;
        [SerializeField]
        private Texture brush_3_Texture;

        [Header("Brush Size")]
		[SerializeField]
		private Text brushSizeText;
		
		[Header("Animation")]
		[SerializeField]
		private ScratchAnimatorEx scratchAnimator;
        [SerializeField]
        private ScratchAnimation _animation_1;
        [SerializeField]
        private ScratchAnimation _animation_2;
        [SerializeField]
        private ScratchAnimation _animation_3;

        private const string TogglePrefsKey = "ScratchCardDemoProgressToggle";
		private const string BrushPrefsKey = "ScratchCardDemoBrush";
		private const string SizePrefsKey = "ScratchCardDemoSize";

		// Toolbar Progress Panel
        public void OnValueChangedScratchProgress(float progress)
        {
            progressText.text = "Progress : " + progress.ToString("F2");
        }

		// ScratchMode Panel
        public void OnValueChangedRightPanelEraseToggle(bool isOn)
		{
			Debug.LogFormat(LOG_FORMAT, "OnValueChangedRightPanelEraseToggle(), isOn : " + isOn);

            if (isOn == true)
            {
                cardManager.Card.Mode = ScratchMode.Erase;
            }
        }

        public void OnValueChangedRightPanelRestoreToggle(bool isOn)
        {
            Debug.LogFormat(LOG_FORMAT, "OnValueChangedRightPanelRestoreToggle(), isOn : " + isOn);

            if (isOn == true)
            {
                cardManager.Card.Mode = ScratchMode.Restore;
            }
        }

        // Brush Panel
        public void OnValueChangedRightPanelBrush_1_Toggle(bool isOn)
		{
			Debug.LogFormat(LOG_FORMAT, "OnValueChangedRightPanelBrush_1_Toggle(), isOn : " + isOn);

			if (isOn == true)
			{
                cardManager.BrushTexture = brush_1_Texture;
            }
        }

        public void OnValueChangedRightPanelBrush_2_Toggle(bool isOn)
        {
            Debug.LogFormat(LOG_FORMAT, "OnValueChangedRightPanelBrush_2_Toggle(), isOn : " + isOn);

            if (isOn == true)
            {
                cardManager.BrushTexture = brush_2_Texture;
            }
        }

        public void OnValueChangedRightPanelBrush_3_Toggle(bool isOn)
        {
            Debug.LogFormat(LOG_FORMAT, "OnValueChangedRightPanelBrush_1_Toggle(), isOn : " + isOn);

            if (isOn == true)
            {
                cardManager.BrushTexture = brush_3_Texture;
            }
        }

		// BrushSize Panel
		public void OnValueChangedRightPanelBrushSizeSlider(float _value)
		{
            cardManager.Card.BrushSize = _value;

			brushSizeText.text = "Brush Size : " + _value.ToString("F2");
        }

		// Animation Panel
		public void OnValueChangedRightPanelAnimation_1_Toggle(bool isOn)
		{
			Debug.LogFormat(LOG_FORMAT, "OnValueChangedRightPanelAnimation_1_Toggle(), isOn : " + isOn);

			if (isOn == true)
			{
                scratchAnimator.Stop();
                scratchAnimator.ScratchAnimation = _animation_1;
            }
		}

        public void OnValueChangedRightPanelAnimation_2_Toggle(bool isOn)
        {
            Debug.LogFormat(LOG_FORMAT, "OnValueChangedRightPanelAnimation_2_Toggle(), isOn : " + isOn);

            if (isOn == true)
            {
                scratchAnimator.Stop();
                scratchAnimator.ScratchAnimation = _animation_2;
            }
        }

        public void OnValueChangedRightPanelAnimation_3_Toggle(bool isOn)
        {
            Debug.LogFormat(LOG_FORMAT, "OnValueChangedRightPanelAnimation_3_Toggle(), isOn : " + isOn);

            if (isOn == true)
            {
                scratchAnimator.Stop();
                scratchAnimator.ScratchAnimation = _animation_3;
            }
        }

		public void OnClickRightPanelPlayButton()
		{
			Debug.LogFormat(LOG_FORMAT, "OnClickRightPanelPlayButton()");

            scratchAnimator.Play();
        }

        public void OnClickRightPanelPauseButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickRightPanelPauseButton()");

            scratchAnimator.Pause();
        }

        public void OnClickRightPanelStopButton()
        {
            Debug.LogFormat(LOG_FORMAT, "OnClickRightPanelStopButton()");

            scratchAnimator.Stop();
        }

		public void OnClickRightPanelResetButton()
		{
			Debug.LogFormat(LOG_FORMAT, "OnClickRightPanelResetButton()");

            cardManager.ClearScratchCard();
        }
	}
}