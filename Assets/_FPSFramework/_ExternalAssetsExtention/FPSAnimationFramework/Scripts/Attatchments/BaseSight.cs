using _KMH_Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework
{
    public class BaseSight : MonoBehaviour
    {
        [SerializeField]
        protected Transform _aimPoint;
        public Transform AimPoint
        {
            get
            {
                return _aimPoint;
            }
        }

        [Space(10)]
        [SerializeField]
        protected bool isModifiable;

        [ReadOnly]
        [SerializeField]
        protected bool _isEquipped = false;
        public bool IsEquipped
        {
            get
            {
                return _isEquipped;
            }
            set
            {
                _isEquipped = value;
                if (value == true &&
                    isModifiable == true)
                {
                    StartCoroutine(PostIsEquippedValueChangedRoutine());
                }
            }
        }

        protected virtual IEnumerator PostIsEquippedValueChangedRoutine()
        {
            yield return null;
        }

        protected virtual void Awake()
        {
            Debug.AssertFormat(AimPoint != null, "this.gameObject.name : " + this.gameObject.name);
        }
    }
}