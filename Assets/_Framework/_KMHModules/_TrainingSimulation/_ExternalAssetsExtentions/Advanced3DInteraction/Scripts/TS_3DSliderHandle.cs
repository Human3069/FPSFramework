using _KMH_Framework;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [RequireComponent(typeof(Collider))]
    public class TS_3DSliderHandle : _3DInteractable
    {
        private const string LOG_FORMAT = "<color=white><b>[TS_3DSliderHandle]</b></color> {0}";
        private const float LIMIT_THRESHOLD = 0.005f;

        [HideInInspector]
        public TS_3DSlider Slider;
        protected Camera _camera;

        protected bool isInitialized = false;

        public override async UniTask WaitUntilCorrectInteract(string _targetValue)
        {
            float targetValue = float.Parse(_targetValue);

            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return Slider.ProgressValue.Similiar(targetValue, LIMIT_THRESHOLD) == true && _camera == null;
            }
        }

        public virtual async UniTask WaitUntilInitialized()
        {
            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return isInitialized == true;
            }
        }

        public void Initialize(TS_3DSlider slider)
        {
            this.Slider = slider;
            this.GetComponent<Collider>().enabled = Slider.enabled;

            isInitialized = true;
        }

        public override void SetSetting(string order)
        {
            Slider.SetSetting(order);
        }

        public override void OnClickDown()
        {
            _camera = CameraEx.Display_0_Camera;

            PostOnPointerDown().Forget();
            Slider.OnGrabbed.Invoke(Slider.ProgressValue);
        }

        protected async UniTaskVoid PostOnPointerDown()
        {
            while (_camera != null)
            {
                Ray _ray = _camera.ScreenPointToRay(Input.mousePosition);
                (Vector3 _pointA, Vector3 _pointB) = Vector3Ex.GetShortestTwoPoint(_ray.origin, _ray.origin + _ray.direction, Slider.StartPos, Slider.EndPos);

                Debug.DrawLine(_pointA, _pointB, Color.yellow);

                Vector3 _evaluatedPointB;
                if (Slider._MovingType == TS_3DSlider.MovingType.None)
                {
                    _evaluatedPointB = _pointB;
                }
                else if (Slider._MovingType == TS_3DSlider.MovingType.Linear)
                {
                    _evaluatedPointB = Vector3.MoveTowards(this.transform.position, _pointB, Slider.MovingSpeed * Time.deltaTime);
                }
                else if (Slider._MovingType == TS_3DSlider.MovingType.EaseOut)
                {
                    _evaluatedPointB = Vector3.Lerp(this.transform.position, _pointB, Slider.MovingSpeed * Time.deltaTime);
                }
                else
                {
                    throw new System.NotImplementedException("");
                }

                this.transform.position = _evaluatedPointB;
                float currentNormalized = Vector3Ex.InverseLerp(Slider.StartPos, Slider.EndPos, this.transform.position);
                float rotationEvaluated = Slider.RotateCurve.Evaluate(currentNormalized);

                this.transform.rotation = Quaternion.Lerp(Slider.StartRot, Slider.EndRot, rotationEvaluated);
                Slider.ProgressValue = currentNormalized;

                if (Slider._AutoReleaseType == TS_3DSlider.AutoReleaseType.ReleaseOnSlidedToLimit &&
                    Slider.ProgressValue >= LIMIT_THRESHOLD)
                {
                    if (Slider._ReturnMode == TS_3DSlider.ReturnMode.ReturnOnNotSlidedToLimit ||
                        Slider._ReturnMode == TS_3DSlider.ReturnMode.ReturnAlways)
                    {
                        PostOnPointerUp().Forget();
                    }
                    
                    // Debug.LogFormat(LOG_FORMAT, "Invoke() - OnRelease, true");
                    Slider.OnReleased.Invoke(Slider.ProgressValue, true);
                    return;
                }

                await UniTask.NextFrame();
            }
        }

        public override void OnClickUpCompletely()
        {
            _camera = null;

            if (Slider._AutoReleaseType == TS_3DSlider.AutoReleaseType.None)
            {
                bool isSlidedToLimit = (Slider.ProgressValue >= LIMIT_THRESHOLD);

                // Debug.LogFormat(LOG_FORMAT, "Invoke() - OnRelease, " + isSlidedToLimit);
                Slider.OnReleased.Invoke(Slider.ProgressValue, isSlidedToLimit);
            }

            if (Slider.ProgressValue >= LIMIT_THRESHOLD)
            {
                if (Slider._ReturnMode == TS_3DSlider.ReturnMode.ReturnAlways)
                {
                    PostOnPointerUp().Forget();
                }
            }
            else
            {
                if (Slider._ReturnMode == TS_3DSlider.ReturnMode.ReturnOnNotSlidedToLimit ||
                    Slider._ReturnMode == TS_3DSlider.ReturnMode.ReturnAlways)
                {
                    PostOnPointerUp().Forget();
                }
            }
        }

        public override void OnClickUpCancelled()
        {
            _camera = null;

            if (Slider._AutoReleaseType == TS_3DSlider.AutoReleaseType.None)
            {
                bool isSlidedToLimit = (Slider.ProgressValue >= LIMIT_THRESHOLD);

                // Debug.LogFormat(LOG_FORMAT, "Invoke() - OnRelease, " + isSlidedToLimit);
                Slider.OnReleased.Invoke(Slider.ProgressValue, isSlidedToLimit);
            }

            if (Slider.ProgressValue >= LIMIT_THRESHOLD)
            {
                if (Slider._ReturnMode == TS_3DSlider.ReturnMode.ReturnAlways)
                {
                    PostOnPointerUp().Forget();
                }
            }
            else
            {
                if (Slider._ReturnMode == TS_3DSlider.ReturnMode.ReturnOnNotSlidedToLimit ||
                    Slider._ReturnMode == TS_3DSlider.ReturnMode.ReturnAlways)
                {
                    PostOnPointerUp().Forget();
                }
            }
        }

        protected async UniTaskVoid PostOnPointerUp()
        {
            _camera = null;
            while (Slider.ProgressValue != 0f &&
                   _camera == null)
            {
                if (Slider._MovingType == TS_3DSlider.MovingType.None)
                {
                    this.transform.position = Slider.StartPos;
                    this.transform.rotation = Slider.StartRot;

                    return;
                }
                else if (Slider._MovingType == TS_3DSlider.MovingType.Linear)
                {
                    this.transform.position = Vector3.MoveTowards(this.transform.position, Slider.StartPos, Slider.MovingSpeed * Time.deltaTime);
                }
                else if (Slider._MovingType == TS_3DSlider.MovingType.EaseOut)
                {
                    this.transform.position = Vector3.Lerp(this.transform.position, Slider.StartPos, Slider.MovingSpeed * Time.deltaTime);
                }

                float currentNormalized = Vector3Ex.InverseLerp(Slider.StartPos, Slider.EndPos, this.transform.position);
                float rotationEvaluated = Slider.RotateCurve.Evaluate(currentNormalized);

                this.transform.rotation = Quaternion.Lerp(Slider.StartRot, Slider.EndRot, rotationEvaluated);
                Slider.ProgressValue = currentNormalized;

                await UniTask.NextFrame();
            }
        }
    }
}