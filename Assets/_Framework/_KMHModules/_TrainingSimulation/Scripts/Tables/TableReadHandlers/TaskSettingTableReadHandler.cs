using System.Collections.Generic;

namespace VTSFramework.TSModule
{
    public class TaskSettingTableReadHandler : BaseTableReadHandler<TaskSettingRow>
    {
        // private const string LOG_FORMAT = "<color=white><b>[TaskSettingTableReadHandler]</b></color> {0}";
       
        protected override bool SortConditionPredicate(TaskSettingRow row)
        {
            string missionType = VTSManager.Instance._MissionType.ToString();

            return row.Mission_Type.Equals(missionType);
        }

        public void SetTaskSetting(int firstIndex, int secondIndex)
        {
            List<TaskSettingRow> equalList = FirstSheet.sortedTableList.FindAll(PredicateFunc);
            bool PredicateFunc(TaskSettingRow row)
            {
                return row.First_Index == firstIndex &&
                       row.Second_Index == secondIndex;
            }

            foreach (TaskSettingRow row in equalList)
            {
                UIDObject uidObj = UIDManager.Instance.GetUIDObject(row.Uid);
                uidObj._BaseObj.SetSetting(row.Order);
            }
        }
    }
}