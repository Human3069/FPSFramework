using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace _KMH_Framework
{
    [ExecuteInEditMode]
    public class ClampedRotaryKnobEx : BaseKnobEx
    {
        private enum InteractionTypes
        {
            VERTICAL_DRAG,
            ROTATION_DRAG
        }

        [Header("=== ClampedRotaryKnobEx ===")]
        [SerializeField]
        private InteractionTypes InteractionType = InteractionTypes.VERTICAL_DRAG;

        [Space(10)]
        [SerializeField]
        private float minAngle = -135f;
        [ReadOnly]
        [SerializeField]
        private float maxAngle;

        [Space(10)]
        [SerializeField]
        private float angleRange = 270f;
        [SerializeField]
        private float rotatedAngle = 0f;

        [ReadOnly]
        [SerializeField]
        private float _normalized = 0f;
        public float Normalized
        {
            get
            {
                return _normalized;
            }
            set
            {
                if (_normalized != value)
                {
                    _normalized = Mathf.Clamp01(value);

                    SetKnobPosition(_normalized);
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
        private UnityEvent<float> _onKnobGrabbedEvent = default;
        [SerializeField]
        private UnityEvent<float> _onValueChangedEvent = default;
        [SerializeField]
        private UnityEvent<float> _onKnobReleasedEvent = default;

        private Vector3 _prevMouseDirection;
        private Vector3 _grabbedMouseOffset;

        protected override void Start()
        {
            if (Application.isPlaying == true)
            {
                base.Start();

                if (isInvokeValueChangedOnStart == true)
                {
                    SetKnobPosition(_normalized);
                    OnValueChanged();
                }
            }
        }

        protected void Update()
        {
            angleRange = Mathf.Clamp(angleRange, 0.1f, float.MaxValue);

            maxAngle = minAngle + angleRange;

            rotatedAngle = Mathf.Clamp(rotatedAngle, minAngle, maxAngle);
            Normalized = (rotatedAngle - minAngle) / angleRange;
        }

        public override void OnGrabbed()
        {
            base.OnGrabbed();
            _grabbedMouseOffset = MousePositionOnRelativePlane() - transform.position;
            _onKnobGrabbedEvent.Invoke(Normalized);

            PostOnGrabbed().Forget();
        }

        protected async UniTaskVoid PostOnGrabbed()
        {
            while (_isGrabbed == true)
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
                rotatedAngle = Mathf.Clamp(rotatedAngle, minAngle, minAngle + angleRange);

                _handle.localEulerAngles = Vector3.up * rotatedAngle;

                Normalized = (rotatedAngle - minAngle) / angleRange;

                await UniTask.NextFrame();
            }
        }

        protected override void OnValueChanged()
        {
            if (Normalized < 0f || Normalized > 1f)
            {
                Debug.LogException(new System.ArgumentOutOfRangeException("Normalized", Normalized, "Setting knob value requires value from [0 - 1]"), this);
                return;
            }

            if (Application.isPlaying == true)
            {
                _onValueChangedEvent.Invoke(Normalized);
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();

            _onKnobReleasedEvent.Invoke(Normalized);
        }

        protected override void SetKnobPosition(float normalized)
        {
            rotatedAngle = Mathf.Lerp(minAngle, minAngle + angleRange, normalized);
            _handle.localEulerAngles = Vector3.up * rotatedAngle;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 startAngle = Quaternion.AngleAxis(minAngle, this.transform.up) * this.transform.forward;
            Vector3 endAngle = Quaternion.AngleAxis(maxAngle, this.transform.up) * this.transform.forward;

            float size = this.transform.localScale.magnitude;
            float radiusIncrement = size / 10f;

            Handles.color = new Color(1f, 0f, 0f, 0.1f);
            float radius = size;
            float filledAngleToDraw = rotatedAngle - minAngle;
            while (filledAngleToDraw > 360f)
            {
                Handles.DrawSolidArc(this.transform.position, this.transform.up, startAngle, 360f, radius);
                filledAngleToDraw -= 360f;
                radius += radiusIncrement;
            }
            Handles.DrawSolidArc(this.transform.position, this.transform.up, startAngle, filledAngleToDraw, radius);

            Handles.color = Color.black;
            radius = size;
            float anglesToDraw = angleRange;
            while (anglesToDraw > 360f)
            {
                Handles.DrawWireArc(this.transform.position, this.transform.up, startAngle, 360f, radius);
                anglesToDraw -= 360f;
                radius += radiusIncrement;
            }
            Handles.DrawWireArc(this.transform.position, this.transform.up, startAngle, anglesToDraw, radius);

            // draw min angle as a line
            Handles.color = Color.yellow;
            Handles.DrawLine(this.transform.position, this.transform.position + (startAngle * radius));

            Handles.color = Color.green;
            Handles.DrawLine(this.transform.position, this.transform.position + (endAngle * radius));
        }
#endif
    }
}
