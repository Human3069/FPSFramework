using Cysharp.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace _KMH_Framework
{
    [ExecuteInEditMode]
    public class UnclampedRotaryKnobEx : BaseKnobEx
    {
        private enum InteractionTypes
        {
            VERTICAL_DRAG,
            ROTATION_DRAG
        }

        [Header("=== UnclampedRotaryKnobEx ===")]
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
        [SerializeField]
        private UnityEvent<int> _onKnobGrabbedEvent = default;
        [SerializeField]
        private UnityEvent<int> _onValueChangedEvent = default;
        [SerializeField]
        private UnityEvent<int> _onKnobReleasedEvent = default;

        private Vector3 _prevMouseDirection;
        private Vector3 _grabbedMouseOffset;

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
            if (Application.isPlaying == false)
            {
                _handle.localEulerAngles = Vector3.up * rotatedAngle;
                RotatedLapIndex = (int)(rotatedAngle / 360f);
            }
        }

        public override void OnGrabbed()
        {
            base.OnGrabbed();
            _grabbedMouseOffset = MousePositionOnRelativePlane() - this.transform.position;
            _onKnobGrabbedEvent.Invoke(RotatedLapIndex);

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

                await UniTask.NextFrame();
            }
        }

        protected override void OnValueChanged()
        {
            if (Application.isPlaying == true)
            {
                _onValueChangedEvent.Invoke(RotatedLapIndex);
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();

            _onKnobReleasedEvent.Invoke(RotatedLapIndex);
        }

        protected override void SetKnobPosition(float normalized)
        {
            // Unclamped Knob은 normalized를 지원하지 않습니다.
        }
    }
}
