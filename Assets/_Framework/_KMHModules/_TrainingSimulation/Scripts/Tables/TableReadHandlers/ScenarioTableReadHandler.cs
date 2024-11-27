using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class ScenarioTableReadHandler : BaseTableReadHandler<TaskRow>
    {
        // private const string LOG_FORMAT = "<color=white><b>[ScenarioTableReadHandler]</b></color> {0}";

        // StepIndex, TaskIndex가 초기화된 리스트
        protected List<TaskRow> currentAllSecondIndexList;
        protected int currentFirstIndex = -1;
        protected int currentSecondIndex = -1;

        public bool HasNextFirstIndex
        {
            get
            {
                List<TaskRow> taskRowList = FirstSheet.sortedTableList.FindAll(PredicateFunc);
                bool PredicateFunc(TaskRow taskRow)
                {
                    bool isNextFirstIndexEquals = taskRow.First_Index == currentFirstIndex + 1;
                    bool isSecondIndexEquals = taskRow.Second_Index == 0;

                    return isNextFirstIndexEquals && isSecondIndexEquals;
                }

                return taskRowList.Count > 0;
            }
        }

        public bool HasNextSecondIndex
        {
            get
            {
                List<TaskRow> taskRowList = FirstSheet.sortedTableList.FindAll(PredicateFunc);
                bool PredicateFunc(TaskRow taskRow)
                {
                    bool isFirstIndexEquals = taskRow.First_Index == currentFirstIndex;
                    bool isNextSecondIndexEquals = taskRow.Second_Index == currentSecondIndex + 1;

                    return isFirstIndexEquals && isNextSecondIndexEquals;
                }

                return taskRowList.Count > 0;
            }
        }

        protected override bool SortConditionPredicate(TaskRow taskRow)
        {
            string missionType = VTSManager.Instance._MissionType.ToString();

            return taskRow.Mission_Type.Equals(missionType);
        }

        public virtual void InitializeTask(int firstIndex, int secondIndex)
        {
            currentFirstIndex = firstIndex;
            currentSecondIndex = secondIndex;

            currentAllSecondIndexList = FirstSheet.sortedTableList.FindAll(PredicateFunc);
            bool PredicateFunc(TaskRow taskRow)
            {
                bool isFirstIndexEquals = taskRow.First_Index == currentFirstIndex;
                bool isSecondIndexEquals = taskRow.Second_Index == currentSecondIndex;

                return isFirstIndexEquals == true && isSecondIndexEquals == true;
            }
        }

        public List<TaskRow> GetSortedList()
        {
            return FirstSheet.sortedTableList;
        }

        public List<TaskRow> GetAllSecondIndexList()
        {
            return currentAllSecondIndexList;
        }
    }
}