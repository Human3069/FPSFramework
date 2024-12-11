using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static FPS_Framework.BulletHandler;

namespace FPS_Framework
{
    public class BulletPredictLogger : MonoBehaviour
    {
        [Space(10)]
        [SerializeField]
        protected float duration = 3.25f;
        [SerializeField]
        protected float snapshotDuration = 0.25f;

        [Space(10)]
        [SerializeField]
        protected List<Predict> predictList = new List<Predict>();

        protected Vector3 enablePos;

        protected void OnEnable()
        {
            enablePos = this.transform.position;

            OnEnableAsync().Forget();
        }

        protected async UniTaskVoid OnEnableAsync()
        {
            UnityEditor.Selection.objects = new Object[] { this.gameObject };

            predictList.Clear();
            float totalDuration = 0f;

            while (totalDuration <= duration)
            {
                Vector3 delta = (enablePos - this.transform.position);
                float length = delta.magnitude; 
                float yDelta = delta.y;
                float zDelta = Mathf.Sqrt(Mathf.Pow(length, 2) - Mathf.Pow(yDelta, 2));

                totalDuration = Mathf.Round(totalDuration * 100f) * 0.01f;

                Debug.Log(totalDuration + " sec, zDelta : " + zDelta + ", yDelta : " + yDelta);
                predictList.Add(new Predict(totalDuration, zDelta, yDelta));

                await UniTask.WaitForSeconds(snapshotDuration);
                totalDuration += snapshotDuration;
            }
        }

        [ContextMenu("Insert Forward Data")]
        protected void InsertForwardData()
        {
            BulletType type = this.GetComponent<BulletHandler>()._BulletType;

            BulletPredictData targetData = Resources.Load<BulletPredictData>(type.ToString() + "_PredictData");
            targetData.ForwardPredicts = predictList.ToArray();
        }

        [ContextMenu("Insert Up Data")]
        protected void InsertUpData()
        {
            BulletType type = this.GetComponent<BulletHandler>()._BulletType;

            BulletPredictData targetData = Resources.Load<BulletPredictData>(type.ToString() + "_PredictData");
            targetData.UpPredicts = predictList.ToArray();
        }
    }
}