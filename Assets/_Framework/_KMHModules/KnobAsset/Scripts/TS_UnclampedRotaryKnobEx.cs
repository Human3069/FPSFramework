using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace VTSFramework.TSModule
{
    [ExecuteInEditMode]
    public class TS_UnclampedRotaryKnobEx : TS_BaseKnobEx
    {
        private enum InteractionTypes
        {
            VERTICAL_DRAG,
            ROTATION_DRAG
        }

        [Header("=== TS_UnclampedRotaryKnobEx ===")]
        [SerializeField]
        private InteractionTypes InteractionType = InteractionTypes.VERTICAL_DRAG;

        [SerializeField]
        private float rotatedAngle = 0f;
        [ReadOnly]
        [SerializeField]
        private int _rotatedLapIndex = 0;
        public int RotatedLapIndex
        {
            get
            {
                return _rotatedLapIndex;
            }
            private set
            {
                if (_rotatedLapIndex != value)
                {
                    _rotatedLapIndex = value;
                    OnValueChanged();
                }
            }
        }

        [Space(10)]
        [SerializeField]
        protected bool isInvokeValueChangedOnStart = true;
        [SerializeField]
        private float mouseDragSensitivity = 50f;

        [Space(10)]
        public UnityEvent<int> OnKnobGrabbedEvent = default;
        public UnityEvent<int> OnValueChangedEvent = default;
        public UnityEvent<int> OnKnobReleasedEvent = default;

        private Vector3 _prevMouseDirection;
        private Vector3 _grabbedMouseOffset;

        public override async UniTask WaitUntilCorrectInteract(string _targetValue)
        {
            int targetValue = int.Parse(_targetValue);

            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return RotatedLapIndex == targetValue;
            }

            UniTaskEx.Cancel(this, 0);
        }

        protected override void Start()
        {
            if (Application.isPlaying == true)
            {
                base.Start();

                if (isInvokeValueChangedOnStart == true)
                {
                    OnValueChanged();
                }
            }
        }

        protected void Update()
        {
            _handle.localEulerAngles = Vector3.up * rotatedAngle;
            RotatedLapIndex = (int)(rotatedAngle / 360f);
        }

        public override void SetSetting(string order)
        {
            if (Application.isPlaying == true)
            {
                rotatedAngle = float.Parse(order);
            }
        }

        public override void OnGrabbed()
        {
            base.OnGrabbed();
            _grabbedMouseOffset = MousePositionOnRelativePlane() - this.transform.position;
            OnKnobGrabbedEvent.Invoke(RotatedLapIndex);

            PostOnGrabbed().Forget();
        }

        protected async UniTaskVoid PostOnGrabbed()
        {
            while(_isGrabbed == true)
            {
                float angleToRotate;
                switch (InteractionType)
                {
                    case InteractionTypes.VERTICAL_DRAG:
                        angleToRotate = (Input.GetAxis("Mouse Y") / Screen.height) * 100f * mouseDragSensitivity;
                        break;

                    case InteractionTypes.ROTATION_DRAG:
                        Vector3 mousePosOnPlane = MousePositionOnRelativePlane() - _grabbedMouseOffset;
                        angleToRotate = Vector3.SignedAngle(_prevMouseDirection - transform.position, mousePosOnPlane - transform.position, transform.up);
                        _prevMouseDirection = mousePosOnPlane;
                        break;

                    default:
                        Debug.LogException(new System.InvalidOperationException("Invalid InteractionTypes value " + InteractionType), this);
                        return;
                }

                rotatedAngle += angleToRotate;
                RotatedLapIndex = (int)(rotatedAngle / 360f);

                _handle.localEulerAngles = Vector3.up * rotatedAngle;

                await UniTaskEx.NextFrame(this, 0);
            }
        }

        protected override void OnValueChanged()
        {
            if (Application.isPlaying == true)
            {
                OnValueChangedEvent.Invoke(RotatedLapIndex);
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();

            OnKnobReleasedEvent.Invoke(RotatedLapIndex);
        }

        protected override void SetKnobPosition(float normalized)
        {
            // Unclamped Knob은 normalized를 지원하지 않습니다.
        }
    }
}
