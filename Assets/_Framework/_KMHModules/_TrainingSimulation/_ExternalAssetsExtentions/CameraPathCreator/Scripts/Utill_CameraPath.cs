using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public class CPC_Visual
    {
        public Color pathColor = Color.green;
        public Color inactivePathColor = Color.gray;
        public Color frustrumColor = Color.white;
        public Color handleColor = Color.yellow;
    }

    public enum CPC_ECurveType
    {
        EaseInAndOut,
        Linear,
        Custom
    }

    [System.Serializable]
    public class CPC_Point
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 handleprev;
        public Vector3 handlenext;
        public CPC_ECurveType curveTypeRotation;
        public AnimationCurve rotationCurve;
        public CPC_ECurveType curveTypePosition;
        public AnimationCurve positionCurve;
        public bool chained;

        public CPC_Point(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
            handleprev = Vector3.back;
            handlenext = Vector3.forward;
            curveTypeRotation = CPC_ECurveType.EaseInAndOut;
            rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            curveTypePosition = CPC_ECurveType.Linear;
            positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
            chained = true;
        }
    }

    public class Utill_CameraPath : BaseObject // CameraPath
    {
        private const string LOG_FORMAT = "<color=white><b>[Utill_CameraPath]</b></color> {0}";

        public Transform targetTransform;
        public Transform target;

        public bool lookAtTarget = false;
   
        public List<CPC_Point> pointList = new List<CPC_Point>();
        public CPC_Visual visual;

        public bool alwaysShow = true;

        private int currentWaypointIndex;
        private float currentTimeInWaypoint;
        private float timePerSegment;

        private bool paused = false;
        private bool playing = false;

        protected virtual void Start()
        {
            if (Camera.main == null)
            {
                Debug.LogError("There is no main camera in the scene!");
            }

            if (lookAtTarget && target == null)
            {
                lookAtTarget = false;
                Debug.LogError("No target selected to look at, defaulting to normal rotation");
            }

            foreach (CPC_Point point in pointList)
            {
                if (point.curveTypeRotation == CPC_ECurveType.EaseInAndOut)
                {
                    point.rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }

                if (point.curveTypeRotation == CPC_ECurveType.Linear)
                {
                    point.rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }

                if (point.curveTypePosition == CPC_ECurveType.EaseInAndOut)
                {
                    point.positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }

                if (point.curveTypePosition == CPC_ECurveType.Linear)
                {
                    point.positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }
            }
        }

        public async UniTask PlayPathAsync()
        {
            float time = PlayerPrefs.GetFloat("CPC_time", 10);
            if (time <= 0)
            {
                time = 0.001f;
            }

            if (targetTransform == null)
            {
                targetTransform = CameraEx.Display_0_Camera.transform;
                Debug.LogError("No camera selected for following path, defaulting to main camera");
            }

            if (playing == false)
            {
                paused = false;
                playing = true;

                await FollowPath(time);
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "이미 이동중이라서 이동 명령을 실행할 수 없습니다.");
            }
        }

        public void StopPath()
        {
            playing = false;
            paused = false;
            StopAllCoroutines();
        }

        public void UpdateTimeInSeconds(float seconds)
        {
            timePerSegment = seconds / pointList.Count - 1;
        }

        public void PausePath()
        {
            paused = true;
            playing = false;
        }

        public void ResumePath()
        {
            if (paused == true)
            {
                playing = true;
            }

            paused = false;
        }

        public bool IsPaused()
        {
            return paused;
        }

        public bool IsPlaying()
        {
            return playing;
        }

        public int GetCurrentWayPoint()
        {
            return currentWaypointIndex;
        }

        public float GetCurrentTimeInWaypoint()
        {
            return currentTimeInWaypoint;
        }

        public void SetCurrentWayPoint(int value)
        {
            currentWaypointIndex = value;
        }

        public void SetCurrentTimeInWaypoint(float value)
        {
            currentTimeInWaypoint = value;
        }

        public void RefreshTransform()
        {
            targetTransform.transform.position = GetBezierPosition(currentWaypointIndex, currentTimeInWaypoint);
            if (lookAtTarget == false)
            {
                targetTransform.transform.rotation = GetLerpRotation(currentWaypointIndex, currentTimeInWaypoint);
            }
            else
            {
                targetTransform.transform.rotation = Quaternion.LookRotation((target.transform.position - targetTransform.transform.position).normalized);
            }
        }

        protected virtual async UniTask FollowPath(float time)
        {
            UpdateTimeInSeconds(time);
            currentWaypointIndex = 0;
            while (currentWaypointIndex < pointList.Count)
            {
                currentTimeInWaypoint = 0;
                while (currentTimeInWaypoint < 1)
                {
                    if (paused == false)
                    {
                        currentTimeInWaypoint += Time.deltaTime / timePerSegment;
                        targetTransform.transform.position = GetBezierPosition(currentWaypointIndex, currentTimeInWaypoint);
                        if (lookAtTarget == false)
                        {
                            targetTransform.transform.rotation = GetLerpRotation(currentWaypointIndex, currentTimeInWaypoint);
                        }
                        else
                        {
                            targetTransform.transform.rotation = Quaternion.LookRotation((target.transform.position - targetTransform.transform.position).normalized);
                        }
                    }

                    await UniTaskEx.NextFrame(nameof(Utill_CameraPath));
                }

                ++currentWaypointIndex;
                if (currentWaypointIndex == pointList.Count - 1)
                {
                    break;
                }
            }

            StopPath();
        }

        protected virtual int GetNextIndex(int index)
        {
            if (index == pointList.Count - 1)
            {
                return 0;
            }

            return index + 1;
        }

        protected virtual Vector3 GetBezierPosition(int pointIndex, float time)
        {
            float evaluated = pointList[pointIndex].positionCurve.Evaluate(time);
            int nextIndex = GetNextIndex(pointIndex);

            return
                Vector3.Lerp(
                    Vector3.Lerp(
                        Vector3.Lerp(pointList[pointIndex].position,
                            pointList[pointIndex].position + pointList[pointIndex].handlenext, evaluated),
                        Vector3.Lerp(pointList[pointIndex].position + pointList[pointIndex].handlenext,
                            pointList[nextIndex].position + pointList[nextIndex].handleprev, evaluated), evaluated),
                    Vector3.Lerp(
                        Vector3.Lerp(pointList[pointIndex].position + pointList[pointIndex].handlenext,
                            pointList[nextIndex].position + pointList[nextIndex].handleprev, evaluated),
                        Vector3.Lerp(pointList[nextIndex].position + pointList[nextIndex].handleprev,
                            pointList[nextIndex].position, evaluated), evaluated), evaluated);
        }

        private Quaternion GetLerpRotation(int pointIndex, float time)
        {
            return Quaternion.LerpUnclamped(pointList[pointIndex].rotation, pointList[GetNextIndex(pointIndex)].rotation, pointList[pointIndex].rotationCurve.Evaluate(time));
        }

#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeGameObject == gameObject || alwaysShow)
            {
                if (pointList.Count >= 2)
                {
                    for (int i = 0; i < pointList.Count; i++)
                    {
                        if (i < pointList.Count - 1)
                        {
                            CPC_Point index = pointList[i];
                            CPC_Point indexNext = pointList[i + 1];
                     
                            Color pathColor;
                            if (UnityEditor.Selection.activeGameObject == this.gameObject)
                            {
                                pathColor = visual.pathColor;
                            }
                            else
                            {
                                pathColor = visual.inactivePathColor;
                            }
                            UnityEditor.Handles.DrawBezier(index.position, indexNext.position, index.position + index.handlenext, indexNext.position + indexNext.handleprev, pathColor, null, 5);
                        }
                    }
                }

                for (int i = 0; i < pointList.Count; i++)
                {
                    CPC_Point index = pointList[i];
                    Gizmos.matrix = Matrix4x4.TRS(index.position, index.rotation, Vector3.one);
                    Gizmos.color = visual.frustrumColor;
                    Gizmos.DrawFrustum(Vector3.zero, 90f, 0.25f, 0.01f, 1.78f);
                    Gizmos.matrix = Matrix4x4.identity;
                }
            }
        }
#endif
    }
}