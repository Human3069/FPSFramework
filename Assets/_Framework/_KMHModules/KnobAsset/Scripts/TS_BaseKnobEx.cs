using _KMH_Framework;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public abstract class TS_BaseKnobEx : _3DInteractable
    {
        [Header("=== TS_BaseKnob ===")]
        [SerializeField]
        protected Transform _handle;

        protected bool _isGrabbed = false;
        protected Camera _camera;

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

            _camera = CameraEx.Display_0_Camera;
        }

        protected abstract void SetKnobPosition(float normalized);

        public override void OnClickDown()
        {
            OnGrabbed();
        }

        public override void OnClickUpCompletely()
        {
            OnReleased();
        }

        public override void OnClickUpCancelled()
        {
            OnReleased();
        }

        public virtual void OnGrabbed()
        {
            Debug.Log("OnGrabbed");
            _isGrabbed = true;
        }

        public virtual void OnReleased()
        {
            Debug.Log("OnReleased");
            _isGrabbed = false;
        }

        protected abstract void OnValueChanged();
  
        protected virtual Vector3 MousePositionOnRelativePlane()
        {
            Plane plane = new Plane(transform.up, transform.position);
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
         
            if (plane.Raycast(ray, out float hitDist))
            {
                Vector3 targetPoint = ray.GetPoint(hitDist);
                return targetPoint;
            }
            return Vector3.zero;
        }
    }
}
