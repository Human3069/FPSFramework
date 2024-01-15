using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPSFramework
{
    [RequireComponent(typeof(Collider))]
    public class BaseInteractable : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[BaseInteractable]</b></color> {0}";

        protected Collider _collider;

        protected virtual void Awake()
        {
            _collider = this.GetComponent<Collider>();
        }

        public virtual void DoInteract()
        {
            Debug.LogFormat(LOG_FORMAT, "DoInteract()");

            //
        }
    }
}