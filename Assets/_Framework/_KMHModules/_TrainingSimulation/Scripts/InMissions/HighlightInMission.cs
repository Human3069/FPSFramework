using Cysharp.Threading.Tasks;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class HighlightInMission : BaseInMission
    {
        // private const string LOG_FORMAT = "<color=white><b>[HighlightInMission]</b></color> {0}";

        protected List<IHighlight> highlightingList = new List<IHighlight>();

        [Header("=== HighlightInMission ===")]
        [SerializeField]
        protected _2DHighlightableData _2DData;
        public static _2DHighlightableData _2DHighlightData;

        [SerializeField]
        protected _3DHighlightableData _3DData;
        public static _3DHighlightableData _3DHighlightData;

        protected override void Awake()
        {
            _2DHighlightData = _2DData;
            _3DHighlightData = _3DData;

            base.Awake();
        }

        public override async UniTask DoInMissionAsync(TaskRow row)
        {
            UIDObject uidObj = UIDManager.Instance.GetUIDObject(row.Uid);
            IHighlight highlight = GetHighlight(uidObj);

            if (row.Direct.Contains("manual") == true)
            {
                highlight._InMissionType = InMissionType.Manual;
                highlight.HighlightDuration = 0f;

                if (row.Parameter.Equals("on") == true)
                {
                    highlight.enabled = true;
                    highlightingList.Add(highlight);
                }
                else
                {
                    Debug.Assert(row.Parameter.Equals("off") == true);
                    highlight.DisableAsync();
                    highlightingList.Remove(highlight);
                }
            }
            else
            {
                highlight._InMissionType = InMissionType.Auto;
                if (string.IsNullOrEmpty(row.Parameter) == true)
                {
                    highlight.HighlightDuration = 0f;
                }
                else
                {
                    highlight.HighlightDuration = float.Parse(row.Parameter);
                }
                highlight.enabled = true;
                highlightingList.Add(highlight);

                await UniTask.WaitForSeconds(highlight.HighlightDuration);
            }
        }

        protected override void OnBeforeInMissionStepChange(int firstIndex, int secondIndex)
        {
            for (int i = highlightingList.Count - 1; i >= 0; i--)
            {
                if (highlightingList[i]._InMissionType == InMissionType.Auto)
                {
                    highlightingList[i].DisableAsync();
                    highlightingList.Remove(highlightingList[i]);
                }
            }
        }

        public virtual void DoInMissionRecorded(string uid)
        {
            if (string.IsNullOrEmpty(uid) == false)
            {
                UIDObject uidObj = UIDManager.Instance.GetUIDObject(uid);
                IHighlight highlight = GetHighlight(uidObj);

                if (highlightingList.Contains(highlight) == false)
                {
                    highlightingList.Add(highlight);
                }

                foreach (IHighlight currentHighlight in highlightingList)
                {
                    if (highlightingList.Contains(currentHighlight) == false)
                    {
                        currentHighlight.DisableAsync();
                    }
                }
            }
            else
            {
                foreach (IHighlight currentHighlight in highlightingList)
                {
                    currentHighlight.DisableAsync();
                }
                highlightingList.Clear();
            }
        }

        public virtual void DisableHighlight(UIDObject uidObj)
        {
            IHighlight highlight = uidObj.GetComponent<IHighlight>();

            if (highlightingList.Contains(highlight) == true)
            {
                highlight.DisableAsync();
                highlightingList.Remove(highlight);
            }
        }

        protected virtual IHighlight GetHighlight(UIDObject uidObj)
        {
            if (uidObj._UidType == UIDObject.UidType._2D)
            {
                if (uidObj.TryGetComponent<_2DHighlight>(out _) == false)
                {
                    uidObj.AddComponent<_2DHighlight>();
                }
            }
            else if (uidObj._UidType == UIDObject.UidType._3D)
            {
                if (uidObj.TryGetComponent<_3DHighlight>(out _) == false)
                {
                    uidObj.AddComponent<_3DHighlight>();
                }
            }
            else
            {
                Debug.Assert(false);
            }

            IHighlight highlight = uidObj.GetComponent<IHighlight>();
            return highlight;
        }
    }
}