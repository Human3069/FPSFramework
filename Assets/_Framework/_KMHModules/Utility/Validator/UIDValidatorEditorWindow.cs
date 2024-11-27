using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VTSFramework.TSModule;

namespace GoogleSheetsToUnity
{
#if UNITY_EDITOR
    public class UIDValidatorEditorWindow : EditorWindow
    {
        private static string LOG_FORMAT = "<color=#37AFB6><b>[UIDValidatorEditorWindow]</b></color> {0}";

        private static UIDValidatorConfig _config;
        private static UIDValidatorConfig config
        {
            get
            {
                if (_config == null)
                {
                    _config = Resources.Load<UIDValidatorConfig>("UIDValidatorConfig");
                }

                return _config;
            }
        }

        // Find Uid Object
        private UIDObject foundUidObj;

        private string uidToFind = "";
        private bool isFound;

        // Validate Overlapped Uid
        private List<TaskRow> StreamingAssetTaskRowList
        {
            get
            {
                string directory = Application.streamingAssetsPath + "/" + config.StreamingAssetsDirectory;
                if (directory.Contains(".json") == false)
                {
                    directory += ".json";
                }

                string json = File.ReadAllText(directory);
                List<TaskRow> taskRowList = JsonConvert.DeserializeObject<List<TaskRow>>(json);
                return taskRowList;
            }
        }

        private List<UIDObject> foundOverlappedUidObjList;

        private string validateOverlappedUIDResult = "-";
        private int _overlappedUidIndex = 0;
        private int OverlappedUidIndex
        {
            get
            {
                return _overlappedUidIndex;
            }
            set
            {
                if (foundOverlappedUidObjList == null || foundOverlappedUidObjList.Count == 0)
                {
                    validateOverlappedUIDResult = "중복 없음";
                }
                else
                {
                    string uid = "[" + foundOverlappedUidObjList[value].UidValue + "]";
                    string nameContainsParent = "[" + foundOverlappedUidObjList[value].transform.parent.name + "/" + foundOverlappedUidObjList[value].name + "]";

                    validateOverlappedUIDResult = "- 중복 UID : " + uid + "\n- 이름 :         " + nameContainsParent + "\n- index :       [" + value + "] of [" + foundOverlappedUidObjList.Count + "]";
                    Selection.activeGameObject = foundOverlappedUidObjList[value].gameObject;
                }

                _overlappedUidIndex = value;
            }
        }

        // Validate Indexes

        [MenuItem("Fov-Studio/FovFramework - UID Validator")]
        private static void Open() 
        {
            UIDValidatorEditorWindow instance = GetWindow<UIDValidatorEditorWindow>("UID Validator");
            instance.Show();
        }

        private void OnDestroy()
        {
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        protected void OnGUI()
        {
            GUIStyle headerStyle = new GUIStyle();
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;

            GUILayout.Label(" UID 오브젝트 검색하기", headerStyle);
            GUILayout.BeginHorizontal();
            uidToFind = EditorGUILayout.TextField("UID : ", uidToFind);

            if (GUILayout.Button("Find UID Object") == true)
            {
                isFound = TryFindUIDObject(out foundUidObj);
            }
            GUILayout.EndHorizontal();

            if (isFound == false)
            {
                GUILayout.Label("해당 UID가 존재하지 않습니다.");
            }
            else
            {
                string uidValue = "[" + foundUidObj.UidValue + "]";
                string nameContainsParent = "[" + foundUidObj.transform.parent.name + "/" + foundUidObj.name + "]";

                GUILayout.Label("찾은 UID : " + uidValue + ", 이름 : " + nameContainsParent);
            }

            GUILayout.Space(20);

            GUILayout.Label(" 모든 중복 UID 찾기", headerStyle);
            if (GUILayout.Button("Validate Overlapped UID") == true)
            {
                ValidateOverlappedUID();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev") == true)
            {
                if (OverlappedUidIndex > 0)
                {
                    OverlappedUidIndex--;
                }
            }

            if (GUILayout.Button("Next") == true)
            {
                if (OverlappedUidIndex < foundOverlappedUidObjList.Count - 1)
                {
                    OverlappedUidIndex++;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.Label(validateOverlappedUIDResult);

            GUILayout.Space(20);

            GUILayout.Label(" 시나리오 테이블과 씬 비교", headerStyle);
            config.StreamingAssetsDirectory = EditorGUILayout.TextField("테이블 주소 : ", config.StreamingAssetsDirectory);
            if (GUILayout.Button("Validate ScenarioTable UIDs") == true)
            {
                ValidateScenarioTableUIDs();
            }

            GUILayout.Space(20);

            GUILayout.Label(" 시나리오 테이블 FirstIndex, SecondIndex 검증", headerStyle);
            if (GUILayout.Button("Validate ScenarioTable Indexes") == true)
            {
                ValidateScenarioTableIndexes();
            }
        }

        private bool TryFindUIDObject(out UIDObject foundUidObj)
        {
            UIDObject[] uidObjs = Resources.FindObjectsOfTypeAll<UIDObject>();
            foreach (UIDObject uidObj in uidObjs)
            {
                if (uidObj.UidValue.ToInMissionText().Equals(uidToFind) == true)
                {
                    Selection.activeGameObject = uidObj.gameObject;
                    foundUidObj = uidObj;

                    return true;
                }
            }

            foundUidObj = null;
            return false;
        }

        private void ValidateOverlappedUID()
        {
            UIDObject[] uidObjs = Resources.FindObjectsOfTypeAll<UIDObject>();

            List<string> uidValueList = new List<string>();
            List<string> overlappedUidValueList = new List<string>();

            foreach (UIDObject uidObj in uidObjs)
            {
                string uid = uidObj.UidValue.ToInMissionText();
                if (uidValueList.Contains(uid) == false)
                {
                    uidValueList.Add(uid);
                }
                else
                {
                    if (overlappedUidValueList.Contains(uid) == false)
                    {
                        overlappedUidValueList.Add(uid);
                    }
                }
            }

            foundOverlappedUidObjList = new List<UIDObject>();

            foreach (string overlappedUidValue in overlappedUidValueList)
            {
                UIDObject[] foundUidObjs = Array.FindAll<UIDObject>(uidObjs, PredicateFunc);
                bool PredicateFunc(UIDObject uidObj)
                {
                    string foundUid = uidObj.UidValue.ToInMissionText();
                    return foundUid.Equals(overlappedUidValue);
                }

                for (int i = foundUidObjs.Length - 1; i >= 0; i--)
                {
                    foundOverlappedUidObjList.Add(foundUidObjs[i]);
                }
            }

            if (foundOverlappedUidObjList.Count == 0)
            {
                validateOverlappedUIDResult = "중복 없음";
            }
            else
            {
                OverlappedUidIndex = 0;
                foreach (UIDObject foundOverlappedUidObj in foundOverlappedUidObjList)
                {
                    string uidValue = "[" + foundOverlappedUidObj.UidValue + "]";
                    string nameContainsParent = "[" + foundOverlappedUidObj.transform.parent.name + "/" + foundOverlappedUidObj.name + "]";

                    Debug.LogFormat(LOG_FORMAT, "중복 UID : " + uidValue + ", 이름 : " + nameContainsParent);
                }
            }
        }

        private void ValidateScenarioTableUIDs()
        {
            List<TaskRow> taskRowList = StreamingAssetTaskRowList;
             
            List<string> allSceneUidValueList = new List<string>();
            List<string> allTableUidValueList = new List<string>();

            List<string> exceptSceneUidValueList = new List<string>();
            List<string> exceptTableUidValueList = new List<string>();
       
            UIDObject[] sceneUidObjs = Resources.FindObjectsOfTypeAll<UIDObject>();
            foreach(UIDObject sceneUidObj in sceneUidObjs)
            {
                string sceneUid = sceneUidObj.UidValue.ToInMissionText();

                if (allSceneUidValueList.Contains(sceneUid) == false &&
                    string.IsNullOrEmpty(sceneUid) == false)
                {
                    allSceneUidValueList.Add(sceneUid);
                    exceptTableUidValueList.Add(sceneUid);
                }
            }

            foreach(TaskRow taskRow in taskRowList)
            {
                string tableUid = taskRow.Uid.ToInMissionText();

                if (allTableUidValueList.Contains(tableUid) == false &&
                    string.IsNullOrEmpty(tableUid) == false)
                {
                    allTableUidValueList.Add(tableUid);
                    exceptSceneUidValueList.Add(tableUid);
                }
            }

            foreach (string sceneUidValue in allSceneUidValueList)
            {
                exceptSceneUidValueList.Remove(sceneUidValue);
            }
            foreach (string tableUidValue in allTableUidValueList)
            {
                exceptTableUidValueList.Remove(tableUidValue);
            }

            foreach (string exceptTableUidValue in exceptTableUidValueList)
            {
                Debug.LogFormat(LOG_FORMAT, "씬O 테이블X : [" + exceptTableUidValue + "]");
            }
            foreach (string exceptSceneUidValue in exceptSceneUidValueList)
            {
                Debug.LogFormat(LOG_FORMAT, "씬X 테이블O : [" + exceptSceneUidValue + "]");
            }
        }

        private void ValidateScenarioTableIndexes()
        {
            List<TaskRow> taskRowList = StreamingAssetTaskRowList;
            Dictionary<string, List<TaskRow>> missionTypeRowDic = new Dictionary<string, List<TaskRow>>();

            foreach (string enumValue in Enum.GetNames(typeof(VTSManager.MissionType)))
            {
                List<TaskRow> typicalTaskRowList = new List<TaskRow>();
                typicalTaskRowList = taskRowList.FindAll(PredicateFunc);
                bool PredicateFunc(TaskRow row)
                {
                    string missionType = row.Mission_Type.ToInMissionText();
                    return missionType.Equals(enumValue);
                }

                missionTypeRowDic.Add(enumValue, typicalTaskRowList);
            }

            int sheetIndex = 2;
            bool isWrong = false;

            foreach (KeyValuePair<string, List<TaskRow>> pair in missionTypeRowDic)
            {
                int missionTypeIndex = 0;
                int currentFirstIndex = -1;
                int currentSecondIndex = -1;

                foreach (TaskRow row in pair.Value)
                {
                    if (missionTypeIndex == 0)
                    {
                        bool isFirstAtAll = row.First_Index == 0 &&
                                            row.Second_Index == 0;

                        if (isFirstAtAll == false)
                        {
                            isWrong = true;
                            Debug.LogErrorFormat(LOG_FORMAT, "[" + sheetIndex + "]번째 테이블 First_Index 및 Second_Index 는 둘 다 0 이어야 합니다.");
                        }
                    }
                    else
                    {
                        bool isEqualBoth = row.First_Index == currentFirstIndex &&
                                           row.Second_Index == currentSecondIndex;

                        bool isFirstAsc = row.First_Index == currentFirstIndex + 1 &&
                                          row.Second_Index == 0;

                        bool isSecondAsc = row.First_Index == currentFirstIndex &&
                                           row.Second_Index == currentSecondIndex + 1;

                        if (isEqualBoth == false && isFirstAsc == false && isSecondAsc == false)
                        {
                            isWrong = true;
                            Debug.LogErrorFormat(LOG_FORMAT, "[" + sheetIndex + "]번째 테이블 First_Index 및 Second_Index 할당이 잘못 되었습니다.");
                        }
                    }

                    missionTypeIndex++;
                    sheetIndex++;

                    currentFirstIndex = row.First_Index;
                    currentSecondIndex = row.Second_Index;
                }
            }

            if (isWrong == false)
            {
                Debug.LogFormat(LOG_FORMAT, "테이블의 First_Index 및 Second_Index 가 깔끔합니다.");
            }
        }
    }
#endif
}