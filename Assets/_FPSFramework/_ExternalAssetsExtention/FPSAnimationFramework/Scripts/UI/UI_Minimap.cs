using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FPS_Framework.ZuluWar
{
    public class UI_Minimap : MonoBehaviour
    {
        [SerializeField]
        protected GameObject onObj;
        [SerializeField]
        protected GameObject offObj;

        protected bool _isOn = false;
        public bool IsOn
        {
            get
            {
                return _isOn;
            }
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;

                    onObj.SetActive(value);
                    offObj.SetActive(!value);
                }
            }
        }

        protected void Awake()
        {
            onObj.SetActive(false);
            offObj.SetActive(true);
        }
    }
}