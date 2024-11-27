using _KMH_Framework;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class InteractionInMission : BaseInMission
    {
        // private const string LOG_FORMAT = "<color=white><b>[InteractionInMission]</b></color> {0}";

        public static List<string> TargetUidList = new List<string>();

        [Header("=== InteractionInMission ===")]
        [SerializeField]
        protected HighlightInMission highlightInMission;
        [SerializeField]
        protected Camera targetCamera;

        protected List<_3DInteractable> onClickDownList;

#if UNITY_EDITOR
        protected List<Vector3> EDITOR_hitPointList = new List<Vector3>();
#endif

        protected override void Awake()
        {
            base.Awake();
            PostAwake().Forget();
        }

        protected async UniTaskVoid PostAwake()
        {
            await UniTask.WaitWhile(() => ConfigurationReader.Instance == null);
            await ConfigurationReader.Instance.WaitUntilReady();

            float maxDistance = ConfigurationReader.Instance.UserConfigHandler.Result.MaxInteractableDistance;
            bool isRaycastAll = ConfigurationReader.Instance.UserConfigHandler.Result.IsRaycastAll;

            List<_3DInteractable> currentHoverList = new List<_3DInteractable>();

            while (true)
            {
                _3DInteractable.HoverList.Clear();

                Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = null;

                if (isRaycastAll == true)
                {
                    hits = Physics.RaycastAll(ray, maxDistance);
                }
                else
                {
                    if (Physics.Raycast(ray, out RaycastHit hit, maxDistance) == true)
                    {
                        hits = new RaycastHit[1];
                        hits[0] = hit;
                    }
                }

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.TryGetComponent<_3DInteractable>(out _3DInteractable baseInteractable) == true)
                    {
                        bool isInteractable;
                        if (TargetUidList.Contains(baseInteractable.UidObj.UidValue) == true)
                        {
                            isInteractable = baseInteractable.IsInteractable == true;
                        }
                        else
                        {
                            isInteractable = baseInteractable.IsInteractable == true &&
                                             baseInteractable.IsInteractableWithoutInMission == true;
                        }

                        if (_3DInteractable.HoverList.Contains(baseInteractable) == false && isInteractable == true)
                        {
                            _3DInteractable.HoverList.Add(baseInteractable);
                            baseInteractable.IsHover = true;
                        }
                    }
                }

                foreach (_3DInteractable baseInteractable in currentHoverList)
                {
                    if (_3DInteractable.HoverList.Contains(baseInteractable) == false)
                    {
                        baseInteractable.IsHover = false;
                    }
                }

                currentHoverList = new List<_3DInteractable>(_3DInteractable.HoverList);
                
                SetClicked(_3DInteractable.HoverList);
                DrawGizmos(ray, hits, maxDistance, isRaycastAll);
                
                await UniTask.NextFrame();
            }
        }

        protected virtual void SetClicked(List<_3DInteractable> hoverList)
        {
            if (Input.GetMouseButtonDown(0) == true)
            {
                onClickDownList = new List<_3DInteractable>(hoverList);
                foreach (_3DInteractable baseInteractable in onClickDownList)
                {
                    baseInteractable.OnClickDown();
                }
            }
            else if (Input.GetMouseButtonUp(0) == true)
            {
                List<_3DInteractable> completedList = onClickDownList.FindAll(baseInteractable => hoverList.Contains(baseInteractable) == true);
                List<_3DInteractable> cancelledList = onClickDownList.FindAll(baseInteractable => hoverList.Contains(baseInteractable) == false);

                foreach (_3DInteractable baseInteractable in completedList)
                {
                    baseInteractable.OnClickUpCompletely();
                }

                foreach (_3DInteractable baseInteractable in cancelledList)
                {
                    baseInteractable.OnClickUpCancelled();
                }

                onClickDownList.Clear();
            }
        }

        public override async UniTask DoInMissionAsync(TaskRow row)
        {
            TargetUidList.Add(row.Uid);

            if (row.Direct.Contains("hl") == true)
            {
                highlightInMission.DoInMissionAsync(row).Forget();
            }

            UIDObject uidObj = UIDManager.Instance.GetUIDObject(row.Uid);
            IInteractable interactable = uidObj._BaseObj as IInteractable;

            await interactable.WaitUntilCorrectInteract(row.Target_Value);

            if (row.Direct.Contains("hl") == true)
            {
                highlightInMission.DisableHighlight(uidObj);
            }

            TargetUidList.Remove(row.Uid);
        }

        protected override void OnBeforeInMissionStepChange(int firstIndex, int secondIndex)
        {
            TargetUidList.Clear();
        }

        protected virtual void DrawGizmos(Ray ray, RaycastHit[] hits, float maxDistance, bool isRaycastAll)
        {
#if UNITY_EDITOR
            if (isRaycastAll == true)
            {
                if (hits.Length > 0)
                {
                    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.green);
                    foreach (RaycastHit hit in hits)
                    {
                        EDITOR_hitPointList.Add(hit.point);
                    }
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
                }
            }
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, maxDistance) == true)
                {
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                    EDITOR_hitPointList.Add(hit.point);
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red);
                }
            }
#endif
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            foreach (Vector3 point in EDITOR_hitPointList)
            {
                Gizmos.DrawSphere(point, 1f);
            }

            EDITOR_hitPointList.Clear();
        }
#endif
    }
}