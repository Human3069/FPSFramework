using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FPS_Framework
{
    public class Seat : MonoBehaviour, ISeatable
    {
        [SerializeField]
        protected Transform _sitTransform;

        [SerializeField]
        protected UnityEvent<bool> _onSeat;

        public void Interact(Transform _transform, bool isSeated)
        {
            Debug.LogFormat(_Log._Format(this), "Interact(), isSeated : " + isSeated);

            _transform.position = _sitTransform.position;

            _onSeat.Invoke(isSeated);
        }
    }
}