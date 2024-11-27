using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoogleSheetsToUnity
{
#if true  //[fov-studio update] 2024.04
    // 사용할때 true로 설정
    [CreateAssetMenu(fileName = "_Base_GSTU_Config", menuName = "ScriptableObjects/_Base_GSTU_Config", order = int.MaxValue)]
#endif

    public class _BaseGoogleSheetsToUnityConfig : GoogleSheetsToUnityConfig
    {
        [Header("[저장할 위치 지정]")]
        public string SavePath = "Assets/";

        [Header("[구글 시트 데이타]")]
        public List<TableData> SheetTableData;
    }

    [Serializable]
    public class TableData
    {
        public string TableName;
        public string SheetUriCode; // _associatedSheet;
        public string WorkSheet;    // _associatedWorksheet;
        public string FileName;     // _saveFileName;
        public string IgnorePrefix; // _ignorePrefix;
        public string Element;      // _element;
    }
}