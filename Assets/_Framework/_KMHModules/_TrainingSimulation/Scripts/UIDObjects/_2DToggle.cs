using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace VTSFramework.TSModule
{
    public class _2DToggle : _2DButton, IToggle
    {
        [Header("=== _2DToggle ===")]
        [SerializeField]
        protected bool _isOn;
        public virtual bool IsOn
        {
            get
            {
                return _isOn;
            }
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    OnValueChangedEvent.Invoke(value);

                    if (value == true && toggleGroup != null)
                    {
                        toggleGroup.SetOffWithout(this);
                    }
                }
            }
        }

        [Space(10)]
        public UnityEvent<bool> OnValueChangedEvent;

        [Space(10)]
        [SerializeField]
        protected VTSToggleGroup toggleGroup;

        public override async UniTask WaitUntilCorrectInteract(string _targetValue)
        {
            if (string.IsNullOrEmpty(_targetValue) == true)
            {
                await base.WaitUntilCorrectInteract(_targetValue);
            }
            else
            {
                isClickedOnWait = false;
                bool targetValue = bool.Parse(_targetValue);

                await UniTask.WaitUntil(PredicateFunc);
                bool PredicateFunc()
                {
                    return isClickedOnWait == true && targetValue == IsOn;
                }

                IsHover = false;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            if (toggleGroup != null)
            {
                toggleGroup.Register(this);
            }
        }

        public override void SetSetting(string order)
        {
            IsOn = bool.Parse(order);
        }

        protected override void OnClickUpCompletely()
        {
            base.OnClickUpCompletely();

            IsOn = !IsOn;
        }

        public virtual void SetWithoutNotify(bool isOn)
        {
            _isOn = isOn;
        }
    }
}