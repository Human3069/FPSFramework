using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public struct TaskSettingRow : IRow<TaskSettingRow>
    {
        public TaskSettingRow Validated()
        {
            Mission_Type = Mission_Type.ToInMissionText();
            Uid = Uid.ToInMissionText();
            Order = Order.ToInMissionText();

            return this;
        }

        [ReadOnly]
        public string Mission_Type;
        [ReadOnly]
        public int First_Index;
        [ReadOnly]
        public int Second_Index;
        
        [Space(10)]
        [ReadOnly]
        public string Uid;
        [ReadOnly]
        public string Order;
    }
}