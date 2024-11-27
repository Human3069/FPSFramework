using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VTSFramework.TSModule
{
    // 해당 클래스는 미션의 갯수가 유동적으로 바뀌는 것을 고려하여 짜여졌으로 Json.Deserialize를 이용한 파싱을 구현하지 않음.
    // 구형 JsonTextReader를 이용해 한 셀씩 읽어들여 처리함.
    public class MissionSettingTableReadHandler : BaseTableReadHandler<MissionSettingRow>
    {
        // private const string LOG_FORMAT = "<color=white><b>[MissionSettingTableReadHandler]</b></color> {0}";

        public override void ReadExcel()
        {
            string directory = Application.streamingAssetsPath + "/" + jsonUri + "/" + FirstSheet.jsonName;
            if (directory.Contains(".json") == false)
            {
                directory += ".json";
            }

            // UID - ObjectList
            string json = File.ReadAllText(directory);
            using (JsonTextReader reader = new JsonTextReader(new StringReader(json)))
            {
                List<object> valueList = null;

                object pastValue = "";
                string currentUid = "";

                while (reader.Read() == true)
                {
                    object currentValue = reader.Value;
                    JsonToken currentTokenType = reader.TokenType;

                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        valueList = new List<object>();

                        pastValue = "";
                        currentUid = "";
                    }
                    else if (string.IsNullOrEmpty(pastValue as string) == false && pastValue.Equals("Uid") == true)
                    {
                        currentUid = currentValue as string;
                    }
                    else if (string.IsNullOrEmpty(pastValue as string) == false && pastValue.Equals("Orders") == true)
                    {
                        valueList.Add(currentValue);
                    }
                    else if (string.IsNullOrEmpty(currentUid) == false && currentTokenType == JsonToken.EndObject)
                    {
                        List<string> orderList = valueList.ConvertAll(ingredientObj => ingredientObj.ToString());
                        MissionSettingRow row = new MissionSettingRow(currentUid, orderList.ToArray());

                        FirstSheet.allTableList.Add(row);
                    }

                    pastValue = currentValue;
                }
            }

            FirstSheet.sortedTableList = FirstSheet.allTableList.FindAll(SortConditionPredicate);
            for (int i = 0; i < FirstSheet.sortedTableList.Count; i++)
            {
                FirstSheet.sortedTableList[i] = FirstSheet.sortedTableList[i].Validated();
            }

            IsJsonReadComplete = true;
        }

        protected override bool SortConditionPredicate(MissionSettingRow row)
        {
            return true;
        }

        public virtual void SetMissionSetting()
        {
            foreach (MissionSettingRow row in FirstSheet.sortedTableList)
            {
                UIDObject uidObj = UIDManager.Instance.GetUIDObject(row.Uid);
                uidObj._BaseObj.SetSetting(row.Order);
            }
        }
    }
}