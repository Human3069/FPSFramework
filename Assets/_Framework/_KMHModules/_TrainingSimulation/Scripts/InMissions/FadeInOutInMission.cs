using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class FadeInOutInMission : BaseInMission
    {
        // private const string LOG_FORMAT = "<color=white><b>[FadeInOutInMission]</b></color> {0}";

        [Header("=== FadeInOutInMission ===")]
        public float FadeInOutDuration;

        public override async UniTask DoInMissionAsync(TaskRow row)
        {
            if (row.Parameter.Equals("out") == true)
            {
                await UI_FadeInOut.Instance.FadeOutAsync(FadeInOutDuration, Color.black);
            }
            else
            {
                Debug.Assert(row.Parameter.Equals("in") == true);
                await UI_FadeInOut.Instance.FadeInAsync(FadeInOutDuration, Color.black);
            }
        }
    }
}