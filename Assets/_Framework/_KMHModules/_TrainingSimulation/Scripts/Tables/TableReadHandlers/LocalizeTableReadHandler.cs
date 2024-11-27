using GoogleSheetsToUnity;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;
using System.Diagnostics;

namespace VTSFramework.TSModule
{
    public class LocalizeTableReadHandler : BaseTableReadHandler<LocalizeRow>
    {
        // private const string LOG_FORMAT = "<color=white><b>[MLocalizeTableReadHandler]</b></color> {0}";

        public enum SheetType
        {
            InMission = 0,
            OutMission = 1,
            Popup = 2
        }

        [System.Obsolete("Use 'InMissionSheet' Property Instead")]
#pragma warning disable CS0809
        public override SingleSheet<LocalizeRow> FirstSheet
        {
            get
            {
                return SheetList[0];
            }
        }
#pragma warning restore

        public virtual SingleSheet<LocalizeRow> InMissionSheet
        {
            get
            {
                return SheetList[(int)SheetType.InMission];
            }
        }

        public virtual SingleSheet<LocalizeRow> OutMissionSheet
        {
            get
            {
                return SheetList[(int)SheetType.OutMission];
            }
        }

        public virtual SingleSheet<LocalizeRow> PopupMissionSheet
        {
            get
            {
                return SheetList[(int)SheetType.Popup];
            }
        }

        protected override bool SortConditionPredicate(LocalizeRow row)
        {
            return true;
        }

        public virtual string GetText(SheetType sheetType, int tableId)
        {
            List<LocalizeRow> targetList = null;
            if (sheetType == SheetType.InMission)
            {
                targetList = InMissionSheet.sortedTableList;
            }
            else if (sheetType == SheetType.OutMission)
            {
                targetList = OutMissionSheet.sortedTableList;
            }
            else if (sheetType == SheetType.Popup)
            {
                targetList = PopupMissionSheet.sortedTableList;
            }
            else
            {
                Debug.Assert(false);
            }

            LocalizeRow row = targetList.Find(PredicateFunc);
            bool PredicateFunc(LocalizeRow row)
            {
                return row.Table_Id == tableId;
            }

            if (VTSManager.Instance._LanguageType.Equals(VTSManager.LanguageType.korean) == true)
            {
                return row.Korean_String;
            }
            else if (VTSManager.Instance._LanguageType.Equals(VTSManager.LanguageType.english) == true)
            {
                return row.English_String;
            }
            else
            {
                Debug.Assert(false);
                return "";
            }
        }
    }
}