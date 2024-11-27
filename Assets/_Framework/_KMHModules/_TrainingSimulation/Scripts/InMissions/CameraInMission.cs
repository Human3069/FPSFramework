using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class CameraInMission : BaseInMission
    {
        private const string LOG_FORMAT = "<color=white><b>[CameraInMission]</b></color> {0}";

        [Header("=== CameraInMission ===")]
        [SerializeField]
        protected Camera targetCamera;

        [Header("Camera Point Components")]
        [SerializeField]
        protected float duration = 1f;
        [SerializeField]
        protected AnimationCurve positionCurve;
        [SerializeField]
        protected AnimationCurve rotationCurve;

        [Space(10)]
        [SerializeField]
        protected FadeInOutInMission fadeInOutInMission;

        public override async UniTask DoInMissionAsync(TaskRow row)
        {
            UIDObject uidObj = UIDManager.Instance.GetUIDObject(row.Uid);
#if DEBUG
            DEBUG_TargetUid = uidObj.UidValue;
#endif

            if (uidObj._BaseObj is Utill_CameraPath)
            {
                Utill_CameraPath cameraPath = uidObj._BaseObj as Utill_CameraPath;

                cameraPath.targetTransform = targetCamera.transform;
                await cameraPath.PlayPathAsync();
            }
            else
            {
                await MoveToPoint(row, uidObj);
            }
        }

        public virtual void DoInMissionRecorded(string uid)
        {
            UniTaskEx.Cancel(nameof(Utill_CameraPath));
            UIDObject uidObj = UIDManager.Instance.GetUIDObject(uid);
#if DEBUG
            DEBUG_TargetUid = uidObj.UidValue;
#endif

            if (uidObj._BaseObj is Utill_CameraPath)
            {
                Utill_CameraPath cameraPath = uidObj._BaseObj as Utill_CameraPath;
                CPC_Point lastPoint = cameraPath.pointList[cameraPath.pointList.Count - 1];

                targetCamera.transform.position = lastPoint.position;
                targetCamera.transform.rotation = lastPoint.rotation;
            }
            else
            {
                targetCamera.transform.position = uidObj.transform.position;
                targetCamera.transform.rotation = uidObj.transform.rotation;
            }
        }

        protected virtual async UniTask MoveToPoint(TaskRow row, UIDObject uidObj)
        {
            Vector3 startPos = targetCamera.transform.position;
            Vector3 endPos = uidObj.transform.position;

            Quaternion startRot = targetCamera.transform.rotation;
            Quaternion endRot = uidObj.transform.rotation;

            if (row.Parameter.Contains("immediately") == true && row.Parameter.Contains("fadeinout") == true)
            {
                float duration = fadeInOutInMission.FadeInOutDuration;
                await UI_FadeInOut.Instance.FadeOutAsync(duration, Color.black);

                targetCamera.transform.position = endPos;
                targetCamera.transform.rotation = endRot;

                await UI_FadeInOut.Instance.FadeInAsync(duration);
            }
            else if (row.Parameter.Equals("immediately") == true)
            {
                targetCamera.transform.position = endPos;
                targetCamera.transform.rotation = endRot;
            }
            else
            {
                float _timer = 0f;
                float _normalized = 0f;
                while (_timer < duration)
                {
                    float posEvaluated = positionCurve.Evaluate(_normalized);
                    float rotEvaluated = rotationCurve.Evaluate(_normalized);

                    targetCamera.transform.position = Vector3.Lerp(startPos, endPos, posEvaluated);
                    targetCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, rotEvaluated);

                    await UniTaskEx.NextFrame(nameof(Utill_CameraPath));

                    _timer += Time.deltaTime;
                    _normalized = _timer / duration;
                }

                targetCamera.transform.position = endPos;
                targetCamera.transform.rotation = endRot;
            }
        }

#if DEBUG
        protected string DEBUG_TargetUid;

        protected virtual void OnGUI()
        {
            if (InMissionHandler.DEBUG_isShowGUI == true)
            {
                GUIStyle titleStyle = new GUIStyle();
                titleStyle.fontSize = 20;
                titleStyle.normal.textColor = Color.red;

                GUIStyle contentStyle = new GUIStyle();
                contentStyle.fontSize = 20;
                contentStyle.normal.textColor = Color.yellow;

                GUI.Label(new Rect(100f, 200f, 100f, 100f), "Camera UID : ", titleStyle);
                GUI.Label(new Rect(250f, 200f, 100f, 100f), DEBUG_TargetUid, contentStyle);
            }
        }
#endif
    }
}