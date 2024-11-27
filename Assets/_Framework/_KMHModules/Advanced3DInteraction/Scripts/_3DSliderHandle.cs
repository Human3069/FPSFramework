using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _KMH_Framework
{
    [RequireComponent(typeof(Collider))]
    public class _3DSliderHandle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private const string LOG_FORMAT = "<color=white><b>[_3DSliderHandle]</b></color> {0}";
        private const float LIMIT_THRESHOLD = 0.995f;

        protected _3DSlider _slider;
        protected Camera _camera;

        public void Initialize(_3DSlider slider)
        {
            Debug.LogFormat(LOG_FORMAT, "Initialize()");

            this._slider = slider;
            this.GetComponent<Collider>().enabled = _slider.enabled;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.LogFormat(LOG_FORMAT, "Invoke() - OnGrabbed");

            _camera = eventData.pressEventCamera;

            PostOnPointerDown().Forget();
            _slider.OnGrabbed.Invoke(_slider.ProgressValue);
        }

        protected async UniTaskVoid PostOnPointerDown()
        {
            while (_camera != null)
            {
                Ray _ray = _camera.ScreenPointToRay(Input.mousePosition);
                (Vector3 _pointA, Vector3 _pointB) = Vector3Ex.GetShortestTwoPoint(_ray.origin, _ray.origin + _ray.direction, _slider.StartPos, _slider.EndPos);

                Debug.DrawLine(_pointA, _pointB, Color.yellow);

                Vector3 _evaluatedPointB;
                if (_slider._MovingType == _3DSlider.MovingType.None)
                {
                    _evaluatedPointB = _pointB;
                }
                else if (_slider._MovingType == _3DSlider.MovingType.Linear)
                {
                    _evaluatedPointB = Vector3.MoveTowards(this.transform.position, _pointB, _slider.MovingSpeed * Time.deltaTime);
                }
                else if (_slider._MovingType == _3DSlider.MovingType.EaseOut)
                {
                    _evaluatedPointB = Vector3.Lerp(this.transform.position, _pointB, _slider.MovingSpeed * Time.deltaTime);
                }
                else
                {
                    throw new System.NotImplementedException("");
                }

                this.transform.position = _evaluatedPointB;
                float currentNormalized = Vector3Ex.InverseLerp(_slider.StartPos, _slider.EndPos, this.transform.position);
                float rotationEvaluated = _slider.RotateCurve.Evaluate(currentNormalized);

                this.transform.rotation = Quaternion.Lerp(_slider.StartRot, _slider.EndRot, rotationEvaluated);
                _slider.ProgressValue = currentNormalized;

                if (_slider._AutoReleaseType == _3DSlider.AutoReleaseType.ReleaseOnSlidedToLimit &&
                    _slider.ProgressValue >= LIMIT_THRESHOLD)
                {
                    if (_slider._ReturnMode == _3DSlider.ReturnMode.ReturnOnNotSlidedToLimit ||
                        _slider._ReturnMode == _3DSlider.ReturnMode.ReturnAlways)
                    {
                        PostOnPointerUp().Forget();
                    }
                    
                    Debug.LogFormat(LOG_FORMAT, "Invoke() - OnRelease, true");
                    _slider.OnReleased.Invoke(_slider.ProgressValue, true);
                    return;
                }

                await UniTask.NextFrame();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _camera = null;

            if (_slider._AutoReleaseType == _3DSlider.AutoReleaseType.None)
            {
                bool isSlidedToLimit = (_slider.ProgressValue >= LIMIT_THRESHOLD);

                Debug.LogFormat(LOG_FORMAT, "Invoke() - OnRelease, " + isSlidedToLimit);
                _slider.OnReleased.Invoke(_slider.ProgressValue, isSlidedToLimit);
            }

            if (_slider.ProgressValue >= LIMIT_THRESHOLD)
            {
                if (_slider._ReturnMode == _3DSlider.ReturnMode.ReturnAlways)
                {
                    PostOnPointerUp().Forget();
                }
            }
            else
            {
                if (_slider._ReturnMode == _3DSlider.ReturnMode.ReturnOnNotSlidedToLimit ||
                    _slider._ReturnMode == _3DSlider.ReturnMode.ReturnAlways)
                {
                    PostOnPointerUp().Forget();
                }
            }
        }

        protected async UniTaskVoid PostOnPointerUp()
        {
            _camera = null;
            while (_slider.ProgressValue != 0f &&
                   _camera == null)
            {
                if (_slider._MovingType == _3DSlider.MovingType.None)
                {
                    this.transform.position = _slider.StartPos;
                    this.transform.rotation = _slider.StartRot;

                    return;
                }
                else if (_slider._MovingType == _3DSlider.MovingType.Linear)
                {
                    this.transform.position = Vector3.MoveTowards(this.transform.position, _slider.StartPos, _slider.MovingSpeed * Time.deltaTime);
                }
                else if (_slider._MovingType == _3DSlider.MovingType.EaseOut)
                {
                    this.transform.position = Vector3.Lerp(this.transform.position, _slider.StartPos, _slider.MovingSpeed * Time.deltaTime);
                }

                float currentNormalized = Vector3Ex.InverseLerp(_slider.StartPos, _slider.EndPos, this.transform.position);
                float rotationEvaluated = _slider.RotateCurve.Evaluate(currentNormalized);

                this.transform.rotation = Quaternion.Lerp(_slider.StartRot, _slider.EndRot, rotationEvaluated);
                _slider.ProgressValue = currentNormalized;

                await UniTask.NextFrame();
            }
        }
    }
}