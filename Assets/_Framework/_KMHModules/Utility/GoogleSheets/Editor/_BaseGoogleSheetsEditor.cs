using GoogleSheetsToUnity;
using GoogleSheetsToUnity.ThirdPary;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace _KMH_Framework._TS_Module
{
    /// <summary>
    /// 메뉴에서 GoogleSheets를 Json Packing하는 에디터윈도우
    /// </summary>
    public class _BaseGoogleSheetsEditor : EditorWindow
    {
        // GSTU_Config를 저장하기 위한 ScriptableObject
        private _BaseRegistGSTU_Config baseRegistGstuConfig;

        private int count = 0;
        private Vector2 scrollPos = Vector2.zero;
        private bool showSecret = false;
        private bool isXml = false;
        private bool isOneRowSkip = false;

        private TableData selectTableData;

        //Window 메뉴에 "GoogleSheets Packing" 항목을 추가한다.
        [MenuItem("Fov-Studio/FovFramework - GoogleSheets Packing Window")]
        static void Init()
        {
            var window = GetWindow<_BaseGoogleSheetsEditor>("GoogleSheet To Packing");
            window.baseRegistGstuConfig = Resources.Load<_BaseRegistGSTU_Config>("_BaseRegistGSTU_Config");
            // window.baseRegistGstuConfig.GstuConfig = Resources.Load<_BaseGoogleSheetsToUnityConfig>("VTS_GSTUConfig");

            // _BaseRegistGSTU.asset이 없을때 새로 생성한다.
            if (window.baseRegistGstuConfig == null)
            {
                window.baseRegistGstuConfig = CreateInstance<_BaseRegistGSTU_Config>(); 
                AssetDatabase.CreateAsset(window.baseRegistGstuConfig, "Assets/_Framework/_KMHModules/Utility/_BaseRegistGSTU_Config.asset");
            }

            window.Show();
        }

        private void OnDestroy()
        {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Separator();
            
            if (baseRegistGstuConfig.GstuConfig != null)
            {
                if (GUILayout.Button("Reset GSTU Config"))
                {
                    baseRegistGstuConfig.GstuConfig = null;
                    SpreadsheetManager.Config = null;
                    EditorGUILayout.EndScrollView();
                    return;
                }
                OnGUI_GoogleSheetsToUnityConfig();
                OnGUI_TableData();
            }
            else
            {
                baseRegistGstuConfig.GstuConfig = Resources.Load<_BaseGoogleSheetsToUnityConfig>("VTS_GSTUConfig");
                if (baseRegistGstuConfig.GstuConfig == null)
                {
                    baseRegistGstuConfig.GstuConfig = (_BaseGoogleSheetsToUnityConfig)EditorGUILayout.ObjectField("GSTU Config", baseRegistGstuConfig.GstuConfig, typeof(ScriptableObject), true);
                }
            }
            SpreadsheetManager.Config = baseRegistGstuConfig.GstuConfig;
            EditorUtility.SetDirty(baseRegistGstuConfig);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 테이블 데이타
        /// </summary>
        void OnGUI_TableData()
        {
            EditorGUILayout.Separator();
            // 저장 위치
            baseRegistGstuConfig.GstuConfig.SavePath = EditorGUILayout.TextField("   저장 위치", baseRegistGstuConfig.GstuConfig.SavePath);

            GUILayout.BeginHorizontal();
            GUILayout.Space(100);
            if (GUILayout.Toggle(isXml, "XML") == true)
            {
                isXml = true;
            }
            if (GUILayout.Toggle(!isXml, "JSON") == true)
            {
                isXml = false;
            }

            isOneRowSkip = GUILayout.Toggle(isOneRowSkip, "First Row Skip");

            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            var items = baseRegistGstuConfig.GstuConfig.SheetTableData;
            foreach (var item in items)
            {
                GUILayout.Label($"[{item.TableName} Table]", EditorStyles.boldLabel);
                item.TableName = EditorGUILayout.TextField("   TableName", item.TableName);             //_associatedSheet
                item.SheetUriCode = EditorGUILayout.TextField("   Sheet Url Code", item.SheetUriCode);  //_associatedSheet
                item.WorkSheet = EditorGUILayout.TextField("   WorkSheet", item.WorkSheet);             //_associatedWorksheet
                item.FileName = EditorGUILayout.TextField("   File Name", item.FileName);               //_saveFileName;
                item.IgnorePrefix = EditorGUILayout.TextField("   Ignore Prefix", item.IgnorePrefix);   //_ignorePrefix

                GUILayout.BeginHorizontal();
                if (isXml == true)
                {
                    item.Element = EditorGUILayout.TextField("   Element", item.Element);               //_element
                }

                if (GUILayout.Button("Packing Save File"))
                {
                    selectTableData = item;
                    StartPacking();
                }
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 구글 시트의 GSTU_Config Data
        /// </summary>
        void OnGUI_GoogleSheetsToUnityConfig()
        {
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("[Google Sheets Connect]", EditorStyles.boldLabel);
            baseRegistGstuConfig.GstuConfig.CLIENT_ID = EditorGUILayout.TextField("   클라이언트 ID", baseRegistGstuConfig.GstuConfig.CLIENT_ID);

            GUILayout.BeginHorizontal();
            if (true == showSecret)
            {
                baseRegistGstuConfig.GstuConfig.CLIENT_SECRET = EditorGUILayout.TextField("   클라이언트 보안 비밀번호", baseRegistGstuConfig.GstuConfig.CLIENT_SECRET);
            }
            else
            {
                baseRegistGstuConfig.GstuConfig.CLIENT_SECRET = EditorGUILayout.PasswordField("   클라이언트 보안 비밀번호", baseRegistGstuConfig.GstuConfig.CLIENT_SECRET);
            }
            showSecret = GUILayout.Toggle(showSecret, "Show");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            baseRegistGstuConfig.GstuConfig.PORT = EditorGUILayout.IntField("   Port number", baseRegistGstuConfig.GstuConfig.PORT);
            if (GUILayout.Button("Connection"))
            {
                GoogleAuthrisationHelper.BuildHttpListener();
            }
            GUILayout.EndHorizontal();
        }

        void StartPacking()
        {
            Debug.Log($"[Packing]================================================================");
            EditorCoroutineRunner.StartCoroutine(UpdateStats(UpdateStringTable));
        }

        IEnumerator UpdateStats(UnityAction<GstuSpreadSheet> callback, bool mergedCells = false)
        {
            count = 0;
            var workSheets = selectTableData.WorkSheet.Split(",");
            while (workSheets.Length > count)
            {
                int cnt = count;
                SpreadsheetManager.Read(new GSTU_Search(selectTableData.SheetUriCode.Trim(), workSheets[count].Trim(), "A1", "ZZ5000"), callback, mergedCells);
                yield return new WaitUntil(() => count > cnt);
            }
        }

        void UpdateStringTable(GstuSpreadSheet ss)
        {
            var fileNames = selectTableData.FileName.Split(",");
            var workSheets = selectTableData.WorkSheet.Split(",");

            // 파일 저장 위치
            var title = fileNames[count].Split(".")[0];
            var filePath = $"{baseRegistGstuConfig.GstuConfig.SavePath}";
            if (baseRegistGstuConfig.GstuConfig.SavePath.Last().Equals('/') == false)
            {
                filePath += '/';
            }
            filePath = $"{filePath}{title}";

            if (isXml == true)
            {
                var document = XmlPacking(workSheets, ss);
                filePath += ".xml";

                AnalyzeFolder(filePath);
                document.Save(filePath);
            }
            else
            {
                var result = JsonPacking(workSheets, ss);
                filePath += ".json";

                AnalyzeFolder(filePath);
                StreamWriter sw = new StreamWriter(filePath);
                sw.Write(result.ToString());
                sw.Flush();
                sw.Close();
            }
            Debug.Log($" {filePath} File Packing Complete ");
            count++;
        }

        /// <summary>
        /// GoggleSheet에 맞춤 XML (YMX)
        /// </summary>
        private XmlDocument XmlPacking(string[] workSheets, GstuSpreadSheet ss)
        {
            XmlDocument document = new XmlDocument();
            XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            document.AppendChild(xmlDeclaration);

            XmlElement itemListElement = document.CreateElement(selectTableData.Element);
            document.AppendChild(itemListElement);

            var ignoreList = selectTableData.IgnorePrefix.Split(",").ToList();
            for (var i = 0; i < ignoreList.Count; i++)
            {
                ignoreList[i] = ignoreList[i].Trim();
            }

            var rowKeyList = ss.rows.primaryDictionary.Keys.ToList();
            for (var i = 1; i < rowKeyList.Count; i++)
            {
                List<GSTU_Cell> row;
                //1열 skip 유/무 결정해야한다.
                if (isOneRowSkip == false)
                {
                    row = ss.rows[rowKeyList[i]];//.Skip(1); 
                }
                else
                {
                    row = ss.rows[rowKeyList[i]].Skip(1).ToList(); 
                }
                XmlElement itemElement = (XmlElement)itemListElement.AppendChild(document.CreateElement("instance"));
                foreach (var column in row)
                {
                    if (ignoreList.Find(x => x.Equals(column.columnId)) == null)
                    {
                        itemElement.SetAttribute($"{column.columnId}", $"{column.value}");
                    }
                }
                itemElement.ToString();
            }
            return document;
        }

        /// <summary>
        /// GoogleSheet에 맞춤 JSON (FOV)
        /// </summary>
        private StringBuilder JsonPacking(string[] workSheet, GstuSpreadSheet ss)
        {
            var ignoreList = selectTableData.IgnorePrefix.Split(",").ToList();
            for (var i = 0; i < ignoreList.Count; i++)
            {
                ignoreList[i] = ignoreList[i].Trim();
            }
            var rowKeyList = ss.rows.primaryDictionary.Keys.ToList();


            StringBuilder result = new StringBuilder();
            result.Append("[\r\n");
            for (var i = 1; i < rowKeyList.Count; i++)
            {
                List<GSTU_Cell> row;
                //1열 skip 유/무 결정해야한다.
                if (isOneRowSkip == false)
                {
                    row = ss.rows[rowKeyList[i]];
                }
                else
                {
                    row = ss.rows[rowKeyList[i]].Skip(1).ToList();
                }

                var retValue = string.Empty;
                result.Append("\t{\r\n");
                var lastColumn = row.Last();



                foreach (var column in row)
                {
                    if (ignoreList.Find(x => x.Equals(column.columnId)) == null)
                    {
                        if (string.IsNullOrEmpty(column.value) == false)
                        {
                            if (IsValueDigit(column.value) == true)
                            {
                                var values = column.value.Split(',');
                                if (values.Length > 1)
                                {
                                    retValue = ColumnValues(column.columnId, values);
                                }
                                else
                                {
                                    retValue = $"\t\t\"{column.columnId}\": {GetColumnByJsonFormat(values[0])},\r\n";
                                }
                            }
                            else
                            {
                                var value = column.value.Replace("\\", "\\\\");
                                value = value.Replace("\"", "\\\"");

                                retValue = $"\t\t\"{column.columnId}\": {GetColumnByJsonFormat(value)},\r\n";
                            }
                            // 마지막 체크해서 ',' 삭제
                            if (lastColumn.Equals(column) == true)
                            {
                                retValue = retValue.Remove(retValue.LastIndexOf(','), 1);
                            }
                            result.Append(retValue);
                        }
                    }
                }
                // 마지막 체크해서 ',' 삭제
                if (i < rowKeyList.Count - 1)
                {
                    result.Append("\t}, \r\n");
                }
                else
                {
                    result.Append("\t} \r\n");
                }
            }
            result.Append("]");
            return result;
        }

        /// <summary>
        /// 배열을 Json형식으로 만든다.
        /// </summary>
        private string ColumnValues(string id, string[] values)
        {
            StringBuilder result = new StringBuilder();
            result.Append($"\t\t\"{id}\": [");
            for (int i = 0; i < values.Length; i++)
            {
                result.Append(GetColumnByJsonFormat(values[i]));
                if (i + 1 < values.Length)
                {
                    result.Append(',');
                }
            }
            result.Append("],\r\n");
            return result.ToString();
        }

        /// <summary>
        /// 숫자와 문자 구분해서 Json형식으로 만든다.
        /// </summary>
        private string GetColumnByJsonFormat(string value)
        {
            if (Regex.IsMatch(value, @"^[-+]?\d*\.?\d+$") == true)
            {
                return $"{value}";
            }
            else
            {
                return $"\"{value}\"";
            }
        }
        private bool IsValueDigit(string value)
        {
            return Regex.IsMatch(value, @"^[-+]?\d*\.?\d+$");
        }

        private static void AnalyzeFolder(string targetPath)
        {
            int slashPlace = targetPath.LastIndexOf("/");

            if (slashPlace > 0)
            {
                targetPath = targetPath.Remove(slashPlace);

                if (!Directory.Exists(targetPath))
                {

                    Directory.CreateDirectory(targetPath);
                }
            }
        }
    }

}