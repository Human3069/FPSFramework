using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class DelayInMission : BaseInMission
    {
        private const string LOG_FORMAT = "<color=white><b>[DelayInMission]</b></color> {0}";
        
        public override async UniTask DoInMissionAsync(TaskRow row)
        {
            int seconds;
            if (int.TryParse(row.Parameter, out seconds) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "테이블의 Parameter 값이 잘못되어있는 듯 합니다. 따라서 파싱이 불가능합니다. [" + row.Parameter + "]");
            }

            await UniTask.WaitForSeconds(seconds);
        }
    }
}