using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
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

    public class CameraPath : MonoBehaviour // CPC_CameraPath
    {
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

            if (targetTransform == null)
            {
                targetTransform = Camera.main.transform;
                Debug.LogError("No camera selected for following path, defaulting to main camera");
            }

            if (lookAtTarget && target == null)
            {
                lookAtTarget = false;
                Debug.LogError("No target selected to look at, defaulting to normal rotation");
            }

            foreach (CPC_Point index in pointList)
            {
                if (index.curveTypeRotation == CPC_ECurveType.EaseInAndOut)
                {
                    index.rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }

                if (index.curveTypeRotation == CPC_ECurveType.Linear)
                {
                    index.rotationCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }

                if (index.curveTypePosition == CPC_ECurveType.EaseInAndOut)
                {
                    index.positionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
                }

                if (index.curveTypePosition == CPC_ECurveType.Linear)
                {
                    index.positionCurve = AnimationCurve.Linear(0, 0, 1, 1);
                }
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PlayPath();
            }
        }
#endif

        public void PlayPath()
        {
            float time = PlayerPrefs.GetFloat("CPC_time", 10);
            if (time <= 0)
            {
                time = 0.001f;
            }

            paused = false;
            playing = true;

            StopAllCoroutines();
            StartCoroutine(FollowPath(time));
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

        protected virtual IEnumerator FollowPath(float time)
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

                    yield return 0;
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