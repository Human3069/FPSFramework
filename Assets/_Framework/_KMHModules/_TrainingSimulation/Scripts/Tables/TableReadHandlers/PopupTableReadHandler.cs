using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class PopupTableReadHandler : BaseTableReadHandler<PopupRow>
    {
        // private const string LOG_FORMAT = "<color=white><b>[ScenarioTableReadHandler]</b></color> {0}";

        protected List<string> popupUidList;
        
        public override void ReadExcel()
        {
            Debug.Assert(VTSManager.Instance.ScenarioTable.IsJsonReadComplete == true);
            Debug.Assert(IsJsonReadComplete == false);

            popupUidList = new List<string>();
            List<TaskRow> scenarioTableList = VTSManager.Instance.ScenarioTable.FirstSheet.sortedTableList;

            foreach (TaskRow row in scenarioTableList)
            {
                if (row.Direct.Contains("popup") == true &&
                    popupUidList.Contains(row.Uid) == false)
                {
                    popupUidList.Add(row.Uid);
                }
            }

            foreach (SingleSheet<PopupRow> sheet in SheetList)
            {
                string _path = Application.streamingAssetsPath + "/" + jsonUri + "/" + sheet.jsonName;
                if (_path.Contains(".json") == false)
                {
                    _path += ".json";
                }
                string _json = File.ReadAllText(_path);

                sheet.allTableList = JsonConvert.DeserializeObject<List<PopupRow>>(_json);
                for (int i = 0; i < sheet.allTableList.Count; i++)
                {
                    sheet.allTableList[i] = sheet.allTableList[i].Validated();
                }
                sheet.sortedTableList = sheet.allTableList.FindAll(SortConditionPredicate);
            }

            IsJsonReadComplete = true;
        }

        protected override bool SortConditionPredicate(PopupRow popupRow)
        {
            return popupUidList.Contains(popupRow.Popup_Uid) == true;
        }
    }
}