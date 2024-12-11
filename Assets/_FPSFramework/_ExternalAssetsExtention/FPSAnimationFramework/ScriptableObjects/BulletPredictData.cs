using UnityEngine;
using static FPS_Framework.BulletHandler;

namespace FPS_Framework
{
    [ExecuteAlways]
    public class BulletPredictData : ScriptableObject
    {
        public BulletType _BulletType;

        [Space(10)]
        public Predict[] ForwardPredicts;
        public Predict[] UpPredicts;
    }

    [System.Serializable]
    public struct Predict
    {
        public Predict(float duration, float forwardDirection, float upDirection)
        {
            this.Duration = duration;

            this.forwardDirection = forwardDirection;
            this.upDirection = upDirection;
        }

        public float Duration;

        [Space(10)]
        public float forwardDirection;
        public float upDirection;
    }
}