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
        protected float duration = 3f;
        [SerializeField]
        protected float snapshotDuration = 0.1f;

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
               
                Debug.Log(totalDuration + " sec, zDelta : " + zDelta + ", yDelta : " + yDelta);
                predictList.Add(new Predict(totalDuration, zDelta, yDelta));

                await UniTask.WaitForSeconds(snapshotDuration);
                totalDuration += snapshotDuration;
            }
        }

        [ContextMenu("Insert Data")]
        protected void InsertData()
        {
            BulletPredictData[] allDatas = Resources.LoadAll<BulletPredictData>("");
            BulletPredictData targetData = null;

            foreach (BulletPredictData data in allDatas)
            {
                if (data._BulletType == this.GetComponent<BulletHandler>()._BulletType)
                {
                    targetData = data;
                    break;
                }
            }

            targetData.Predicts = predictList.ToArray();
        }
    }
}