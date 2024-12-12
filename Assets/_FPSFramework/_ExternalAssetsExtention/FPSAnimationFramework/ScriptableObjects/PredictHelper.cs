using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FPS_Framework.BulletHandler;

namespace FPS_Framework
{
    public static class PredictHelper
    {
        private static Dictionary<BulletType, BulletPredictData> predictDataDic = new Dictionary<BulletType, BulletPredictData>();

        private static void LoadPredictData(BulletType type)
        {
            if (predictDataDic.ContainsKey(type) == false)
            {
                predictDataDic.Add(type, Resources.Load<BulletPredictData>(type.ToString() + "_PredictData"));
            }
            else if (predictDataDic[type] == null)
            {
                predictDataDic[type] = Resources.Load<BulletPredictData>(type.ToString() + "_PredictData");
            }
        }

        public static Vector3[] LoadPredictPoints(BulletType type, Transform firePosT)
        {
            LoadPredictData(type);

            Predict[] forwardPredicts = predictDataDic[type].ForwardPredicts;
            Predict[] upwardPredicts = predictDataDic[type].UpPredicts;
            Vector3[] points = new Vector3[forwardPredicts.Length];

            float normal = Mathf.Pow(Vector3.Dot(firePosT.forward, Vector3.up), 18f);
            
            for (int i = 0; i < forwardPredicts.Length; i++)
            {
                float normalizedForward = Mathf.Lerp(forwardPredicts[i].forwardDirection, upwardPredicts[i].forwardDirection, normal);
                float normalizedUpward = Mathf.Lerp(forwardPredicts[i].upDirection, upwardPredicts[i].upDirection, normal);

                Vector3 point = firePosT.position + (firePosT.forward * normalizedForward) - (Vector3.up * normalizedUpward);
                points[i] = point;
            }

            return points;
        }
    }
}