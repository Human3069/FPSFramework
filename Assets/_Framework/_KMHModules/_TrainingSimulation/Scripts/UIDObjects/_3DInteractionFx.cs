using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public class _3DInteractionFx 
    {
        protected _3DInteractable _interactable;

        protected string CancelKey
        {
            get
            {
                return _interactable.GetInstanceID() + "_" + this.GetType();
            }
        }

        [SerializeField]
        protected AnimationCurve onHoverCurve = AnimationCurve.Linear(0f, 1f, 0.1f, 1.05f);
        protected float hoverTime;

        protected float _hoverScale = 1f;
        protected float HoverScale
        {
            get
            {
                return _hoverScale;
            }
            set
            {
                _hoverScale = value;
                _interactable.transform.localScale = new Vector3(HoverScale * ClickScale, HoverScale * ClickScale, HoverScale * ClickScale);
            }
        }

        [SerializeField]
        protected AnimationCurve onClickCurve = AnimationCurve.Linear(0f, 1f, 0.25f, 1.1f);
        protected float clickTime;

        protected float _clickScale = 1f;
        protected float ClickScale
        {
            get
            {
                return _clickScale;
            }
            set
            {
                if (Application.isPlaying == true)
                {
                    _clickScale = value;
                    _interactable.transform.localScale = new Vector3(HoverScale * ClickScale, HoverScale * ClickScale, HoverScale * ClickScale);
                }
            }
        }

        public virtual async UniTaskVoid Initialize(_3DInteractable interactable)
        {
            this._interactable = interactable;
            this._interactable.OnHoverEvent.AddListener(OnHover);

            if (_interactable is _3DButton)
            {
                _3DButton button = _interactable as _3DButton;
                button.OnClickDownEvent.AddListener(OnClickDown);
                button.OnClickUpCompletedEvent.AddListener(OnClickUp);
                button.OnClickUpCancelledEvent.AddListener(OnClickUp);
            }

            if (_interactable is _3DDelayButton)
            {
                _3DDelayButton delayButton = _interactable as _3DDelayButton;
                delayButton.OnClickDownEvent.AddListener(OnClickDown);
                delayButton.OnClickUpCompletelyEvent.AddListener(OnClickUp);
                delayButton.OnClickUpCancelledEvent.AddListener(OnClickUp);
            }

            if (_interactable is TS_3DSliderHandle)
            {
                TS_3DSliderHandle sliderHandle = _interactable as TS_3DSliderHandle;
                await sliderHandle.WaitUntilInitialized();

                TS_3DSlider slider = sliderHandle.Slider;

                slider.OnGrabbed.AddListener(OnClickDown);
                slider.OnReleased.AddListener(OnClickUp);
            }

            if (_interactable is TS_BaseKnobEx)
            {
                if (_interactable is TS_ClampedRotaryKnobEx)
                {
                    TS_ClampedRotaryKnobEx knob = _interactable as TS_ClampedRotaryKnobEx;
                    
                    knob.OnKnobGrabbedEvent.AddListener(OnClickDown);
                    knob.OnKnobReleasedEvent.AddListener(OnClickUp);
                }
                else if (_interactable is TS_FixedRotaryKnobEx)
                {
                    TS_FixedRotaryKnobEx knob = _interactable as TS_FixedRotaryKnobEx;
                    
                    knob.OnKnobGrabbedEvent.AddListener(OnClickDown);
                    knob.OnKnobReleasedEvent.AddListener(OnClickUp);
                }
                else if (_interactable is TS_UnclampedRotaryKnobEx)
                {
                    TS_UnclampedRotaryKnobEx knob = _interactable as TS_UnclampedRotaryKnobEx;

                    knob.OnKnobGrabbedEvent.AddListener(OnClickDown);
                    knob.OnKnobReleasedEvent.AddListener(OnClickUp);
                }
                else
                {
                    Debug.Assert(false);
                }
            }
        }

        protected virtual void OnHover(bool isOn)
        {
            OnHoverAsync(isOn).Forget();
        }

        protected virtual void OnClickDown()
        {
            OnClickAsync(true).Forget();
        }

        protected virtual void OnClickDown(float value)
        {
            OnClickDown();
        }

        protected virtual void OnClickDown(int value)
        {
            OnClickDown();
        }

        protected virtual void OnClickUp()
        {
            OnClickAsync(false).Forget();
        }

        protected virtual void OnClickUp(float value)
        {
            OnClickUp();
        }

        protected virtual void OnClickUp(int value)
        {
            OnClickUp();
        }

        protected virtual void OnClickUp(float value, bool isCompletely)
        {
            OnClickUp();
        }

        protected virtual async UniTaskVoid OnHoverAsync(bool isOn)
        {
            UniTaskEx.Cancel(CancelKey + "_Hover");

            float maxTime = onHoverCurve.keys[onHoverCurve.keys.Length - 1].time;
            float targetTime = isOn ? maxTime : 0f;

            while (hoverTime != targetTime)
            {
                HoverScale = onHoverCurve.Evaluate(hoverTime);

                await UniTaskEx.NextFrame(CancelKey + "_Hover");
                hoverTime = Mathf.MoveTowards(hoverTime, targetTime, Time.deltaTime);
            }

            hoverTime = targetTime;
            HoverScale = onHoverCurve.Evaluate(hoverTime);
        }

        protected virtual async UniTaskVoid OnClickAsync(bool isDown)
        {
            UniTaskEx.Cancel(CancelKey + "_Click");

            float maxTime = onClickCurve.keys[onClickCurve.keys.Length - 1].time;
            float targetTime = isDown ? maxTime : 0f;

            while (clickTime != maxTime)
            {
                ClickScale = onClickCurve.Evaluate(clickTime);

                await UniTaskEx.NextFrame(CancelKey + "_Click");
                clickTime = Mathf.MoveTowards(clickTime, targetTime, Time.deltaTime);
            }

            clickTime = targetTime;
            ClickScale = onClickCurve.Evaluate(clickTime);
        }
    }
}