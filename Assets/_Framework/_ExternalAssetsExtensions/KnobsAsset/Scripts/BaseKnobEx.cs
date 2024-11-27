using UnityEngine;

namespace _KMH_Framework
{
    public abstract class BaseKnobEx : MonoBehaviour
    {
        [Header("=== BaseKnobEx ===")]
        [SerializeField]
        protected Transform _handle;
        [SerializeField]
        private float HandleGrabbedScaleMultiplier = 1.1f;

        protected bool _isGrabbed = false;
        private Vector3 _baseScale;

        protected virtual void Start()
        {
            if (_handle == null)
            {
                if (this.transform.TryFind("Obj_Handle", out _handle) == false)
                {
                    Debug.LogException(new MissingComponentException("Knob needs to have a child called \"handle\""), this);
                    return;
                }
            }

            _baseScale = _handle.localScale;
        }

        protected abstract void SetKnobPosition(float percentValue);

        public void OnMouseDown()
        {
            OnGrabbed();
        }

        private void OnMouseUp()
        {
            OnReleased();
        }

        public virtual void OnGrabbed()
        {
            _isGrabbed = true;
            _handle.localScale = _baseScale * HandleGrabbedScaleMultiplier;
        }

        public virtual void OnReleased()
        {
            _handle.localScale = _baseScale;
            _isGrabbed = false;
        }

        protected abstract void OnValueChanged();
  
        protected Vector3 MousePositionOnRelativePlane()
        {
            Plane plane = new Plane(transform.up, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out float hitDist))
            {
                Vector3 targetPoint = ray.GetPoint(hitDist);
                return targetPoint;
            }
            return Vector3.zero;
        }
    }
}
