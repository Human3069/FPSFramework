using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace _KMH_Framework
{
    public static class _Log
    {
        public static string _Format(object baseObject)
        {
            return _Format(baseObject.GetType().Name, Color.white);
        }

        public static string _Format(object baseObject, Color formatColor)
        {
            return _Format(baseObject.GetType().Name, formatColor);
        }

        public static string _Format(string nameOfClass)
        {
            return _Format(nameOfClass, Color.white);
        }

        public static string _Format(string nameOfClass, Color formatColor)
        {
            string colorToHex = formatColor.ToHexString();
            string _result = "<color=#" + colorToHex + " ><b>[" + nameOfClass + "]</b></color> {0}";

            return _result;
        }
    }

    public static class _Physical
    {
        public static float GetAngleFromThreePositions(Vector3 pos1, Vector3 pos2, Vector3 pos3)
        {
            Vector3 L1 = pos1 - pos2;
            Vector3 L2 = pos3 - pos2;

            float dotProducts = (L1.normalized.x * L2.normalized.x) + (L1.normalized.y * L2.normalized.y) + (L1.normalized.z * L2.normalized.z);
            double angle = (Mathf.Acos(dotProducts) * 180.0) / Mathf.PI;
            float result = Mathf.Round((float)angle * 1000) / 1000;

            return result;
        }
    }

    public static class _Asynchronous
    {
        public delegate void Rebound(float threshold);
        public static Rebound OnRebound;

        private static bool isReboundBreak = false;

        public static IEnumerator DoRebound(float fadeInTimer, float fadeOutTimer, Rebound _onRebound)
        {
            isReboundBreak = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            isReboundBreak = false;

            OnRebound = _onRebound;

            float _threshold = 0;
            float _timer = fadeInTimer;
            while (_timer > 0)
            {
                OnRebound(_threshold);
                _threshold = Mathf.Lerp(_threshold, 1.0f, Time.deltaTime * 6 / fadeInTimer);

                _timer -= Time.deltaTime;
                yield return null;

                if (isReboundBreak == true)
                {
                    OnRebound = null;
                    yield break;
                }
            }

            _threshold = 1;
            _timer = fadeOutTimer;
            while (_timer > 0)
            {
                OnRebound(_threshold);
                _threshold = Mathf.Lerp(_threshold, 0.0f, Time.deltaTime * 6 / fadeInTimer);

                _timer -= Time.deltaTime;
                yield return null;

                if (isReboundBreak == true)
                {
                    OnRebound = null;
                    yield break;
                }
            }

            _threshold = 0;
            OnRebound(_threshold);
            OnRebound = null;
        }
    }
}