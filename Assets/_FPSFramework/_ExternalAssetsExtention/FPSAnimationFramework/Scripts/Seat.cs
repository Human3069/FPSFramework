using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FPS_Framework
{
    public class Seat : MonoBehaviour, ISeatable
    {
        [Header("Seat")]
        [SerializeField]
        protected Transform _seatTransform;
        [SerializeField]
        protected UnityEvent<bool> _onSeat;

        [ReadOnly]
        [SerializeField]
        protected bool _isSeated = false;
        public virtual bool IsSeated
        {
            get
            {
                return _isSeated;
            }
            protected set
            {
                if (_isSeated != value)
                {
                    _isSeated = value;
                }
            }
        }

        public virtual void Interact(Transform _transform, bool isSeated)
        {
            IsSeated = isSeated;

            _transform.position = _seatTransform.position;
            _onSeat.Invoke(isSeated);
        }
    }
}