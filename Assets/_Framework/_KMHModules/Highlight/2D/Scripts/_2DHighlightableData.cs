using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public class _2DHighlightableData
    {
        public float FlickeringDuration = 0.5f;

        [Space(10)]
        public Color FromColor;
        public Color ToColor;
    }
}