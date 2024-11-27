using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public class TEST_KnobEventReceive : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[TEST_KnobEventReceive]</b></color> {0}";

        [ReadOnly]
        [SerializeField]
        protected bool isShowLog = true;

        public void OnKnobGrabbed(float value)
        {
            if (isShowLog == true)
            {
                Debug.LogFormat(LOG_FORMAT, "OnKnobGrabbed(), value : <color=white><b>" + value + "</b></color>");
            }
        }

        public void OnKnobValueChanged(float value)
        {
            if (isShowLog == true)
            {
                Debug.LogFormat(LOG_FORMAT, "OnKnobValueChanged(), value : <color=white><b>" + value + "</b></color>");
            }
        }

        public void OnKnobReleased(float value)
        {
            if (isShowLog == true)
            {
                Debug.LogFormat(LOG_FORMAT, "OnKnobReleased(), value : <color=white><b>" + value + "</b></color>");
            }
        }
    }
}