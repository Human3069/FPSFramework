using UnityEngine;
using static FPS_Framework.BulletHandler;

namespace FPS_Framework
{
    public class BulletPredictData : ScriptableObject
    {
        public BulletType _BulletType;
        public Predict[] Predicts;

        [ContextMenu("Reset Magnitude")]
        protected void AddMagnitude()
        {
            for (int i = 0; i < Predicts.Length; i++)
            {
                Predict newPredict = new Predict(Predicts[i].Duration, Predicts[i].zDirection, Predicts[i].yDirection);
                Predicts[i] = newPredict;
            }
        }
    }

    [System.Serializable]
    public struct Predict
    {
        public Predict(float duration, float zDirection, float yDirection)
        {
            this.Duration = duration;

            this.zDirection = zDirection;
            this.yDirection = yDirection;
            this.Magnitude = new Vector2(zDirection, yDirection).magnitude;
        }

        public float Duration;

        [Space(10)]
        public float zDirection;
        public float yDirection;
        [ReadOnly]
        public float Magnitude;
    }
}