using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    // 하이라이트, 팝업 등에 사용.
    public enum InMissionType
    {
        Auto,   // 다음 스텝 넘어가면 자동 종료.
        Manual  // 수동으로 테이블 상에서 종료를 지정해야 함.
    }

    public abstract class BaseInMission : MonoBehaviour
    {
        // private const string LOG_FORMAT = "<color=white><b>[BaseInMission]</b></color> {0}";

        [Header("=== BaseInMission ===")]
        [SerializeField]
        protected string _direct;
        public string Direct
        {
            get
            {
                return _direct;
            }
            private set
            {
                _direct = value;
            }
        }

        protected virtual void Awake()
        {
            string correctDirect = Direct.ToInMissionText();
            if (Direct.Equals(correctDirect) == false)
            {
                Direct = correctDirect;
            }

            InMissionHandler.OnBeforeInMissionStepChange += OnBeforeInMissionStepChange;
        }

        protected virtual void OnDestroy()
        {
            InMissionHandler.OnBeforeInMissionStepChange -= OnBeforeInMissionStepChange;
        }

        public abstract UniTask DoInMissionAsync(TaskRow row);

        protected virtual void OnBeforeInMissionStepChange(int firstIndex, int secondIndex) {}

        public virtual bool IsDirectContains(string direct)
        {
            string[] directs = Direct.Split("||");

            List<string> directList = new List<string>();
            foreach (string _direct in directs)
            {
                directList.Add(_direct);
                directList.Add(_direct + "auto");
                directList.Add(_direct + "manual");
            }

            return directList.Contains(direct.ToInMissionText());
        }
    }
}