using UnityEngine;
using UnityEngine.Events;

namespace _KMH_Framework
{
    public class _3DSlider : MonoBehaviour
    {
        // private const string LOG_FORMAT = "<color=white><b>[_3DSlider]</b></color> {0}";

        [SerializeField]
        protected _3DSliderHandle handle;
        [SerializeField]
        protected Transform startTransform;
        [SerializeField]
        protected Transform endTransform;

        public enum ReturnMode
        {
            None,
            ReturnOnNotSlidedToLimit,
            ReturnAlways
        }

        public enum AutoReleaseType
        {
            None,
            ReleaseOnSlidedToLimit,
        }

        public enum MovingType
        {
            None,
            Linear,
            EaseOut
        }

        [Space(10)]
        public ReturnMode _ReturnMode = ReturnMode.None;
        public MovingType _MovingType = MovingType.Linear;
        public AutoReleaseType _AutoReleaseType = AutoReleaseType.None;
        public float MovingSpeed = 1f;

        [Space(10)]
        public AnimationCurve RotateCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space(10)]
        [SerializeField]
        protected float _progressValue;
        public float ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = Mathf.Clamp01(value);
                }
            }
        }

        [SerializeField]
        protected bool _isInteractable = true;
        public bool IsInteractable
        {
            get
            {
                return _isInteractable;
            }
            set
            {
                _isInteractable = value;
                handle.GetComponent<Collider>().enabled = value;
            }
        }

        [Space(10)]
        public UnityEvent<float> OnGrabbed;
        public UnityEvent<float, bool> OnReleased;

        #region Shortcut Properties...
        public Vector3 StartPos
        {
            get
            {
                return startTransform.position;
            }
        }

        public Vector3 EndPos
        {
            get
            {
                return endTransform.position;
            }
        }

        protected Vector3 HandlePos
        {
            get
            {
                return handle.transform.position;
            }
            set
            {
                handle.transform.position = value;
            }
        }

        public Quaternion StartRot
        {
            get
            {
                return startTransform.rotation;
            }
        }

        public Quaternion EndRot
        {
            get
            {
                return endTransform.rotation;
            }
        }

        protected Quaternion HandleRot
        {
            get
            {
                return handle.transform.rotation;
            }
            set
            {
                handle.transform.rotation = value;
            }
        }
        #endregion

        protected void Awake()
        {
            HandlePos = Vector3.Lerp(StartPos, EndPos, ProgressValue);
            HandleRot = Quaternion.Lerp(StartRot, EndRot, ProgressValue);

            handle.Initialize(this);
        }

        protected void OnDrawGizmos()
        {
            if (startTransform != null && endTransform != null)
            {
                ProgressValue = Mathf.Clamp01(ProgressValue);
                if (Application.isPlaying == false)
                {
                    HandlePos = Vector3.Lerp(StartPos, EndPos, ProgressValue);
                    HandleRot = Quaternion.Lerp(StartRot, EndRot, ProgressValue);

                    handle.GetComponent<Collider>().enabled = IsInteractable;
                }

                Gizmos.color = Color.red;
                Gizmos.DrawLine(StartPos, EndPos);
            }
        }
    }
}