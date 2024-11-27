using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public class _2DInteractionFx
    {
        protected _2DInteractable _interactable;

        [SerializeField]
        protected Image hoverImage;
        [SerializeField]
        protected Image onImage;

        [Space(10)]
        [SerializeField]
        protected float hoverTransitionDuration = 0.1f;
        [SerializeField]
        protected float onTransitionDuration = 0.1f;

        protected string CancelKey
        {
            get
            {
                return _interactable.GetInstanceID() + "_" + this.GetType();
            }
        }

        public virtual void Initialize(_2DInteractable interactable)
        {
            this._interactable = interactable;

            if (hoverImage != null)
            {
                hoverImage.color = new Color(hoverImage.color.r, hoverImage.color.g, hoverImage.color.b, 0f);
            }

            if (onImage != null)
            {
                onImage.color = new Color(onImage.color.r, onImage.color.g, onImage.color.b, 0f);
            }

            _interactable.OnHoverEvent.AddListener(OnHover);

            if (_interactable is _2DToggle)
            {
                _2DToggle toggle = _interactable as _2DToggle;
                toggle.OnValueChangedEvent.AddListener(OnValueChanged);
            }
            else if (_interactable is _2DButton)
            {
                _2DButton button = _interactable as _2DButton;
                button.OnClickDownEvent.AddListener(OnClickDown);
                button.OnClickUpCompletedEvent.AddListener(OnClickUp);
                button.OnClickUpCancelledEvent.AddListener(OnClickUp);
            }
            else if (_interactable is _2DDelayButton)
            {
                _2DDelayButton delayButton = _interactable as _2DDelayButton;
                delayButton.OnClickDownEvent.AddListener(OnClickDown);
                delayButton.OnClickUpCompletelyEvent.AddListener(OnClickUp);
                delayButton.OnClickUpCancelledEvent.AddListener(OnClickUp);
            }
            else if (_interactable is _2DInputField)
            {
                _2DInputField inputField = _interactable as _2DInputField;
                inputField.OnSelectAction = OnValueChanged;
            }
        }

        protected virtual void OnHover(bool isOn)
        {
            if (hoverImage != null)
            {
                TransitionAsync(hoverImage, hoverTransitionDuration, isOn).Forget();
            }
        }

        protected virtual void OnClickDown()
        {
            if (onImage != null)
            {
                TransitionAsync(onImage, onTransitionDuration, true).Forget();
            }
        }

        protected virtual void OnClickUp()
        {
            if (onImage != null)
            {
                TransitionAsync(onImage, onTransitionDuration, false).Forget();
            }
        }

        protected virtual void OnClickUp(float value)
        {
            OnClickUp();
        }

        protected virtual void OnValueChanged(bool isOn)
        {
            if (onImage != null)
            {
                TransitionAsync(onImage, onTransitionDuration, isOn).Forget();
            }
        }

        protected virtual async UniTaskVoid TransitionAsync(Image image, float totalDuration, bool isOn)
        {
            UniTaskEx.Cancel(CancelKey + image.name);

            float startAlpha = image.color.a;
            float endAlpha = isOn ? 1f : 0f;

            float timer = 0;
            while (timer < totalDuration)
            {
                float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / totalDuration);
                image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

                await UniTaskEx.NextFrame(CancelKey + image.name);
                timer += Time.deltaTime;
            }

            image.color = new Color(image.color.r, image.color.g, image.color.b, endAlpha);
        }
    }
}