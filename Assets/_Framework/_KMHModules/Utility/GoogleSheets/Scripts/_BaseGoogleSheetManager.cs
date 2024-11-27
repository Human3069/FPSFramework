using System;
using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework._TS_Module
{
    /// <summary>
    /// 테이블을 관리하는 매니저 클래스 <para/>
    /// [fov-studio update] 2024.04 <para/>
    /// </summary>
    public class _BaseGoogleSheetManager : MonoSingleton<_BaseGoogleSheetManager>
    {
        private static string LOG_FORMAT = "<color=#94B530><b>[GoogleSheetManager]</b></color> {0}";

        [Header("[Table Path]")]
        public string JsonFolderPath = "Assets/_Framework/Base_Framework/Asset StoreEx/Google Sheets to Unity/Download Example/LocalRes";

        protected Dictionary<Enum, _ABaseTable> googleSheetDictionary = new Dictionary<Enum, _ABaseTable>();

        public override void Awake()
        {
        }
        protected void Invoke_OnLoadDone()
        {
            Debug.LogWarningFormat(LOG_FORMAT, "<b><color=yellow>Invoke_OnLoadDone()</color></b>");
            DontDestroyOnLoad(gameObject);
        }


        public virtual void AddSheetData(Enum tableType, _ABaseTable table)
        {
            if (googleSheetDictionary.ContainsKey(tableType) == false)
            {
                googleSheetDictionary.Add(tableType, table);
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, $" <color=red> 중복 테이블 [Enum {tableType}] </color>");
            }
        }

        public virtual List<T> GetDataList<T>(Enum tableType)
        {
            if (googleSheetDictionary.TryGetValue(tableType, out var table) == true)
            {
                return table.GetDataList<T>();
            }
            return null;
        }

        public virtual _ABaseTable GetTable(Enum tableType)
        {
            if (googleSheetDictionary.TryGetValue(tableType, out var table) == true)
            {
                return table;
            }
            return null;
        }
    }
}