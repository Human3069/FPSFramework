using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    public class KeyframeHandler : MonoBehaviour
    {
        public Action<int> OnKeyframeReached;

        public void KeyframeReached(int index)
        {
            OnKeyframeReached.Invoke(index);
        }
    }
}