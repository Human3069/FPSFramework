using System;
using UnityEngine;

// This class is created for the example scene. There is no support for this script.
namespace UnityStandardAssets.Utility
{
    [Serializable]
    public class CurveControlledBobEx
    {
        [SerializeField]
        private float horizontalBobRange = 0.33f;
        [SerializeField]
        private float verticalBobRange = 0.33f;
        [SerializeField]
        private AnimationCurve bobCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                             new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                             new Keyframe(2f, 0f)); // sin curve for head bob

        [SerializeField]
        private float ratio = 1f;

        private float _cyclePositionX;
        private float _cyclePositionY;
        private float _bobBaseInterval;
        private Vector3 _originalCameraPosition;
        private float _time;

        public void Setup(Transform camTransform, float bobBaseInterval)
        {
            _bobBaseInterval = bobBaseInterval;
            _originalCameraPosition = camTransform.localPosition;

            // get the length of the curve in time
            _time = bobCurve[bobCurve.length - 1].time;
        }


        public Vector3 DoHeadBob(float speed)
        {
            float xPos = _originalCameraPosition.x + (bobCurve.Evaluate(_cyclePositionX)*horizontalBobRange);
            float yPos = _originalCameraPosition.y + (bobCurve.Evaluate(_cyclePositionY)*verticalBobRange);

            _cyclePositionX += (speed*Time.deltaTime)/_bobBaseInterval;
            _cyclePositionY += ((speed*Time.deltaTime)/_bobBaseInterval)*ratio;

            if (_cyclePositionX > _time)
            {
                _cyclePositionX = _cyclePositionX - _time;
            }
            if (_cyclePositionY > _time)
            {
                _cyclePositionY = _cyclePositionY - _time;
            }

            return new Vector3(xPos, yPos, 0f);
        }
    }
}
