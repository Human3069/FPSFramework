using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public struct TaskRow : IRow<TaskRow>
    {
        public TaskRow Validated()
        {
            Mission_Type = Mission_Type.ToInMissionText();
            Record_Point = Record_Point.ToInMissionText();

            Direct = Direct.ToInMissionText();
            Uid = Uid.ToInMissionText();
            Parameter = Parameter.ToInMissionText();

            return this;
        }

        [ReadOnly]
        public string Mission_Type;
        [ReadOnly]
        public string Record_Point;
        [ReadOnly]
        public int First_Index;
        [ReadOnly]
        public int Second_Index;

        [Space(10)]
        [ReadOnly] 
        public int Localize_Table_Id;

        [Space(10)]
        [ReadOnly] 
        public string Direct;
        [ReadOnly] 
        public string Uid;
        [ReadOnly] 
        public string Parameter;
        [ReadOnly] 
        public string Target_Value;
    }
}