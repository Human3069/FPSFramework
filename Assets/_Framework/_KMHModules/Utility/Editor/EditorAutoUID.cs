using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _KMH_Framework._TS_Module
{
    public class EditorAutoUID : EditorWindow
    {
#if false
        public enum EUIDState
        {
            None,
            New,
            Modify,
        }

        [Serializable]
        public class SaveData
        {
            public string path;
            public string uid;
            public string objName;
            public EUIDState state;
            public string detail;

            public string oldUID;
            public string newUID;

            [NonSerialized]
            public bool isCheck;
        }
        [Serializable]
        public class Save
        {
            public string sceneName;
            public List<SaveData> saveData;
        }


        private List<TextMeshProUGUI> tmpComponents;
        private int selectIdx;

        static public readonly string _sceneName = "AutoUID";
        public readonly string _2d = "2d_";
        public readonly string _3d = "3d_";
        public readonly string _ut = "ut_";
        public readonly string _text = "text_";
        public readonly string _lineSkip = "\r\n";
        public readonly string savePath = "AutoUID";

        public string saveResult;
        public Vector2 scrollPosition;
        public bool isTextID = false;
        public bool isFindText = false;
        public string changeStringID;
        public string findTextID;

        private List<UIDObject> duplicateList = new List<UIDObject>();
        private Action showGuiAction;
        private Dictionary<string, string> dicPrefabByPrefixName = new Dictionary<string, string>();

        [MenuItem("Fov-Studio/Auto UID Window")]
        public static void StartWindow()
        {
            if (SceneManager.GetActiveScene().name.Contains(_sceneName) == true)
            {
                var window = GetWindow<EditorAutoUID>("Auto UID");
                window.maxSize = new Vector2(750, 500);
                window.minSize = new Vector2(750, 500);
                window.Init();
                window.Show();
            }
            else
            {
                EditorUtility.DisplayDialog("Auto UID", "\t\nAutoUID Scene이 아닙니다.\r\nAutoUID Scene에서 실행하세요.", "Yes");
            }
        }
        public void Init()
        {
            isTextID = false;
            isFindText = false;

            findTextID = string.Empty;
            changeStringID = string.Empty;
            saveResult = string.Empty;
            FindObjectInit();

            dicPrefabByPrefixName.Clear();
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach(var root in  rootGameObjects)
            {
                string ab = string.Empty;
                FindNormalPrefixName(root, ref ab);
            }

            foreach (var key in dicPrefabByPrefixName.Keys)
            {
                $"Find key: {key}, value: {dicPrefabByPrefixName[key]}".Log();
            }
        }

        /// <summary>
        /// 프리팹위치 까지 PrefixName을 셋팅해서 프리팹오브젝트을 키로 저장
        /// </summary>
        private void FindNormalPrefixName(GameObject gameObject, ref string prefixName)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(gameObject) == true && PrefabUtility.IsAnyPrefabInstanceRoot(gameObject) == true)
            {
                if (dicPrefabByPrefixName.ContainsKey(gameObject.name) == false)
                {
                    dicPrefabByPrefixName.Add(gameObject.name, prefixName);
                }
                return;
            }

            // Find Prefab Root Object
            if (gameObject.TryGetComponent<RegAutoUID>(out var value) == true)
            {
                prefixName += string.IsNullOrEmpty(prefixName) == false ? "_" : string.Empty;
                prefixName += $"{value._prefixName}";
            }
            // 재귀호출: 하위 오브젝트에서 찾는다
            foreach (Transform child in gameObject.transform)
            {
                FindNormalPrefixName(child.gameObject, ref prefixName);
            }
        }

        // 초기 셋팅
        private void FindObjectInit()
        {
            selectIdx = 0;
            if (tmpComponents?.Count > 0)
            {
                Selection.activeGameObject = tmpComponents[selectIdx].gameObject;
            }
        }
        private string GetStringIDComponent(TextMeshProUGUI objTmp)
        {
            return objTmp.GetComponent<TMPLocalizer>()?.TableId.ToString();
        }

        private bool CheckStringID(GameObject obj, int findStringID)
        {
            if (obj.name.ValueDigit(out var value) == true)
            {
                return value == findStringID;
            }
            else
            {
                if (obj.TryGetComponent<TMPLocalizer>(out var component) == true)
                {
                    return component.Equals(findStringID.ToString());
                }
            }
            return false;
        }
        /// <summary>
        /// UID를 발급한다.
        /// </summary>
        public void SetComponent<T>(GameObject obj, Action<T> resultCall) where T : Component
        {
            T addComponent = null;
            if (obj != null)
            {
                addComponent = obj.GetComponent<T>();
                if (addComponent == null)
                {
                    addComponent = obj.AddComponent<T>();
                }
                EditorUtility.SetDirty(addComponent);
            }
            resultCall?.Invoke(addComponent);
        }

        private bool ActiveDialog(string title, string detail)
        {
            return EditorUtility.DisplayDialog($"{title}", $"{detail}", "Yes", "no");
        }

        private void SetListCount<T>(int value, List<T> t) where T : Component
        {
            selectIdx += value;
            if (selectIdx >= t.Count)
            {
                selectIdx = t.Count - 1;
            }
            if (selectIdx < 0)
            {
                selectIdx = 0;
            }
        }


        private void OnGUI()
        {
            // Layout
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(250), GUILayout.Height(40)) == true)
            {
                Init();
                saveResult = $"Updated Hierarchy information.";
            }
            GUILayout.Space(20);
            var isTextIDValue = GUILayout.Toggle(isTextID, " Text ID", GUILayout.Width(250), GUILayout.Height(40));
            if (isTextIDValue != isTextID)
            {
                saveResult = string.Empty;
                isFindText = false;
                findTextID = string.Empty;
                if (isTextIDValue == true)
                {
                    SetGuiShowAction(GuiFindStringID);
                }
            }
            isTextID = isTextIDValue;
            GUILayout.EndHorizontal();

            // Button
            GUILayout.Space(20);

            if (isTextID == true)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Set String ID", GUILayout.Width(300), GUILayout.Height(50)) == true)
                {
                    SetGuiShowAction(null);
                    findTextID = string.Empty;
                    GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                    saveResult = SetSceneTextID(rootGameObjects, _text);
                }

                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                GUILayout.Label("String ID");
                findTextID = GUILayout.TextField(findTextID);
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                if (GUILayout.Button("Find String ID"))
                {
                    OnGuiFindStringID();
                }
                showGuiAction?.Invoke();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("2D UID"))
                {
                    if (ActiveDialog("2D UID Issuance", "2D Object의 UID를 발급하시겠습니까?") == true)
                    {
                        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                        var data = SetSceneRegUID(rootGameObjects, _2d);
                        saveResult = SaveCSV($"{data.sceneName}_2D", "2D UID", data);
                        SetGuiShowAction(null);
                    }
                }
                if (GUILayout.Button("3D UID"))
                {
                    if (ActiveDialog("3D UID Issuance", "3D Object의 UID를 발급하시겠습니까?") == true)
                    {
                        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                        var data = SetSceneRegUID(rootGameObjects, _3d);
                        saveResult = SaveCSV($"{data.sceneName}_3D", "3D UID", data);
                        SetGuiShowAction(null);
                    }
                }
                if (GUILayout.Button("UT UID"))
                {
                    if (ActiveDialog("UT UID Issuance", "UT Object의 UID를 발급하시겠습니까?") == true)
                    {
                        GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                        var data = SetSceneRegUID(rootGameObjects, _ut);
                        saveResult = SaveCSV($"{data.sceneName}_UT", "UT UID", data);
                        SetGuiShowAction(null);
                    }
                }
                if (GUILayout.Button("같은 아이디 체크"))
                {
                    GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
                    List<UIDObject> rootList = new List<UIDObject>();
                    foreach (var root in rootGameObjects)
                    {
                        var list = root.GetComponentsInChildren<UIDObject>(true);
                        if (list != null && list.Length > 0)
                        {
                            rootList.AddRange(list);
                        }
                    }
                    var duplicates = rootList.GroupBy(x => x.UidValue).Where(x => x.Count() > 1).Select(x => x).ToList();
                    if (duplicates.Count > 0)
                    {
                        saveResult = SaveDuplicateUID(duplicates);
                        SetGuiShowAction(GuiDuplicateShow);
                    }
                    else
                    {
                        saveResult = $"현재 씬에서 중복 UID 없음.";
                    }
                }
                GUILayout.EndHorizontal();
                showGuiAction?.Invoke();
            }


            GUILayout.Space(50);
            if (string.IsNullOrEmpty(saveResult) == false)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Label("[LOG]");
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));
            {
                GUILayout.Label(saveResult);
            }
            GUILayout.EndScrollView();
        }
        private void SetGuiShowAction(Action newAction)
        {
            showGuiAction = newAction;
        }

        private void GuiDuplicateShow()
        {
            GUILayout.Space(30);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label($"\t\t* Duplicate UID:[{duplicateList[selectIdx].UidValue}] ");

            GuiNextObjectShow<UIDObject>(duplicateList, "Duplicate UID");
        }

        private void GuiFindStringID()
        {
            if (isFindText == true)
            {
                if (tmpComponents.Count <= selectIdx)
                {
                    SetListCount(-1, tmpComponents);
                    isFindText = tmpComponents.Count > 0;
                }
                else
                {
                    GUILayout.Space(20);
                    var component = tmpComponents[selectIdx];
                    var stringID = GetStringIDComponent(component);
                    GUILayout.Label($"<<< Find String ID [{stringID}] >>>");
                    GUILayout.Label($"[Path] {GetGameObjectPath(component.gameObject)}");
                    GUILayout.Label($"[String ID] {stringID}");

                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"[Object Name And String ID Change]");
                    changeStringID = GUILayout.TextField($"{changeStringID}", GUILayout.Width(200));
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10);
                    if (GUILayout.Button("현재 오브젝트를 이름, 스트링아이디 변경"))
                    {
                        Selection.activeGameObject = tmpComponents[selectIdx].gameObject;
                        if (SetChangeSelectTextID(tmpComponents[selectIdx].gameObject, changeStringID) == true)
                        {
                            tmpComponents.RemoveAt(selectIdx);
                            changeStringID = string.Empty;
                            FindObjectInit();
                        }
                    }

                    GuiNextObjectShow(tmpComponents, "String ID");
                }
            }
        }
        /// <summary>
        /// Find String ID Button Click Event
        /// </summary>
        private void OnGuiFindStringID()
        {
            if (string.IsNullOrEmpty(findTextID) == false && findTextID.ValueDigit(out var value) == true)
            {
                var list = FindObjectsOfType<TextMeshProUGUI>(true).ToList();
                if (list != null)
                {
                    tmpComponents = list.FindAll(x => CheckStringID(x.gameObject, value) == true);
                    if (tmpComponents != null && tmpComponents.Count > 0)
                    {
                        FindObjectInit();
                        isFindText = tmpComponents.Count > 0;
                        return;
                    }
                }
            }
            EditorUtility.DisplayDialog("Not Found", $"{findTextID} String ID를 찾을 수 없습니다.", "Yes");
        }
        private void GuiNextObjectShow<T>(List<T> t, string currentName) where T : Component
        {
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<<"))
            {
                SetListCount(-1, t);
                Selection.activeGameObject = t[selectIdx].gameObject;
            }
            GUIStyle centeredStyle = new GUIStyle(EditorStyles.label);
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField($"{currentName}: {selectIdx + 1} / {t.Count}", centeredStyle);
            if (GUILayout.Button(">>"))
            {
                SetListCount(1, t);
                Selection.activeGameObject = t[selectIdx].gameObject;
            }
            GUILayout.EndHorizontal();
        }


        private Save SetSceneRegUID(GameObject[] sceneInObjects, string substringToFind)
        {
            List<GameObject> prefabsInScene = new List<GameObject>();
            foreach (GameObject rootGameObject in sceneInObjects)
            {
                var str = GetGameObjectRootPrefab(rootGameObject);
                if (string.IsNullOrEmpty(str) == false)
                {
                    $"...................{rootGameObject.name}: {str}".Log();
                }
                FindPrefabsRecursive(rootGameObject, prefabsInScene);
            }

            Save newSave = new Save()
            {
                sceneName = SceneManager.GetActiveScene().name,
                saveData = new List<SaveData>()
            };

            foreach (GameObject prefabVariant in prefabsInScene)
            {
                var variantOri = FindObjectOriPrefab(prefabVariant);
                List<Transform> findOriTrnList = FindObjectsByNameRecursive<Transform>(variantOri, substringToFind);
                foreach (var oriChild in findOriTrnList)
                {
                    var oriPath = GetGameObjectRootDeletePath(oriChild.gameObject);
                    var prefixName = GetPrefixName(prefabVariant, oriPath);

                    // 컴포넌트 및 UID 생성
                    var uid = $"{prefixName}{oriChild.gameObject.name.Replace(substringToFind, string.Empty)}";
                    SetComponent<UIDObject>(oriChild.gameObject, (component) =>
                    {
                        if (component != null)
                        {
                            component.UidValue = uid;
                        }
                    });
                    // UID 및 Data 기록
                    newSave.saveData.Add(new SaveData()
                    {
                        path = $"{variantOri.name}/{oriPath}",
                        uid = uid,
                        objName = oriChild.name
                    });
                }
                PrefabUtility.SavePrefabAsset(variantOri);
            }
            return newSave;
        }
        /// <summary>
        /// 모든 TextMeshProUGUI 오브젝트를 찾아서 String ID를 적용.
        /// </summary>
        private string SetSceneTextID(GameObject[] sceneInObjects, string substringToFind)
        {
            List<GameObject> prefabsInScene = new List<GameObject>();
            foreach (GameObject rootGameObject in sceneInObjects)
            {
                FindPrefabsRecursive(rootGameObject, prefabsInScene);
            }
            string missing = "";
            foreach (GameObject prefabVariant in prefabsInScene)
            {
                var variantOri = FindObjectOriPrefab(prefabVariant);
                var findOriTrnList = FindObjectsByNameRecursive<TextMeshProUGUI>(variantOri, substringToFind);
                foreach (var oriChild in findOriTrnList)
                {
                    if (oriChild.name.ValueDigit(out var value) == true)
                    {
                        SetComponent<TMPLocalizer>(oriChild.gameObject, (component) =>
                        {
                            component.TableId = value;
                            Debug.Log($"{oriChild.name}: [{value}]");
                        });
                    }
                    else
                    {
                        var path = GetGameObjectRootDeletePath(oriChild.gameObject);
                        missing += $"\t- {oriChild.name}\t[PATH] {path}{_lineSkip}";
                    }
                }
                PrefabUtility.SavePrefabAsset(variantOri);
                if (string.IsNullOrEmpty(missing) == false)
                {
                    missing = $"[String Table ID가 없는 TMP]{_lineSkip}" + missing;
                }
            }
            return missing;
        }


        /// <summary>
        /// 선택한 TMP 오브젝트의 이름과 String ID를 변경
        /// </summary>
        private bool SetChangeSelectTextID(GameObject obj, string stringID)
        {
            if (string.IsNullOrEmpty(stringID) == false)
            {
                var variant = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
                var variantPath = GetGameObjectRootDeletePath(obj);

                var variantOri = FindObjectOriPrefab(variant);
                var findOriTrn = variantOri.transform.Find(variantPath);

                if (findOriTrn == null)
                {
                    EditorUtility.DisplayDialog($"String ID", $"ART Prefab과 Path가 [{variantPath}] 틀립니다.", "Yes");
                }
                else
                {
                    // 240620 곽명환 : TMPLocalizer StringId 필드는 int 값을 사용하므로 파싱하는 부분 추가했습니다
                    int parsedStringId = int.Parse(stringID);

                    findOriTrn.gameObject.name = $"Text_{stringID}";
                    SetComponent<TMPLocalizer>(findOriTrn.gameObject, (component) =>
                    {
                        component.TableId = parsedStringId;
                    });
                    PrefabUtility.SavePrefabAsset(variantOri);
                    return true;
                }
            }
            return false;
        }

        private void SaveCsv(string saveName, Save save)
        {
            CsvFile.WriteStreamingAssets($"{savePath}/{saveName}", (wr) =>
            {
                wr.WriteLine("index,uid,path,object name,state,detail, old, new");
                int index = 0;
                foreach (var data in save.saveData)
                {
                    wr.WriteLine("{0},{1},{2},{3},{4},{5},{6}, {7}", index++, data.uid, data.path, data.objName, data.state, data.detail, data.oldUID, data.newUID);
                }
                wr.Close();
            });
        }
        private string SaveDuplicateUID(List<IGrouping<string, UIDObject>> duplicates)
        {
            string result = "";
            duplicateList.Clear();

            CsvFile.WriteStreamingAssets($"{savePath}/Duplicate", (wr) =>
            {
                wr.WriteLine("group,duplicate uid,path,object name");
                int group = 0;
                foreach (var duplicate in duplicates)
                {
                    result += $"* Duplicate UID: {duplicate.Key}{_lineSkip}";
                    foreach (var data in duplicate)
                    {
                        duplicateList.Add(data);
                        var path = GetGameObjectPath(data.gameObject);
                        result += $"\t- Path: {path}{_lineSkip}";
                        wr.WriteLine("{0},{1},{2},{3}", group, duplicate.Key, path, data.gameObject.name);
                    }
                    group++;
                }
                wr.Close();
                Selection.activeGameObject = duplicateList[0].gameObject;
            });
            return result;
        }
        private string SaveCSV(string name, string title, Save newSave)
        {
            // 전 csv FileLoad
            var oldSave = new Save()
            {
                saveData = new List<SaveData>()
            };
            if (CsvFile.LoadStreamingAssets($"{savePath}/{name}", true, (value) =>
            {
                oldSave.saveData.Add(new SaveData()
                {
                    uid = value[1],
                    path = value[2],
                    objName = value[3],
                });
            }) == false)
            {
                // 전 파일이 없으면 파일 저장 후 종료
                newSave.saveData.ForEach(x => x.state = EUIDState.New);
                SaveCsv(name, newSave);
                return $"[{title} 완료 갯수] - {newSave.saveData.Count}{_lineSkip}\t비교할 전 파일이 없습니다.";
            }

            oldSave.saveData.ForEach(data => { data.isCheck = false; });

            string result = "", modify = "", delete = "", add = "";
            int modifyCnt = 0, addCnt = 0;
            foreach (var newData in newSave.saveData)
            {
                var pathIdx = oldSave.saveData.FindIndex(x => x.path.Equals(newData.path));
                var uidIdx = oldSave.saveData.FindIndex(x => x.uid.Equals(newData.uid));
                var objNameIdx = oldSave.saveData.FindIndex(x => x.objName.Equals(newData.objName));

                if (pathIdx == uidIdx && objNameIdx == uidIdx)
                {
                    if (pathIdx != -1)
                    {
                        // 변경없음
                        Debug.Log($"[변경없음] Paht: [{newData.path}], UID: [{newData.path}]");
                        newData.detail = "-";
                        var oldData = oldSave.saveData[pathIdx];
                        oldData.isCheck = true;
                        oldData.state = EUIDState.None;
                    }
                    else
                    {
                        // 추가
                        newData.state = EUIDState.New;
                        add += $"\t{newData.objName} {_lineSkip}\t\t- [Path]: {newData.path}{_lineSkip}\t\t- [UID]: {newData.uid}{_lineSkip}";
                        addCnt++;
                    }
                }
                else
                {
                    var idx = Math.Max(Math.Max(pathIdx, uidIdx), objNameIdx);
                    var oldData = oldSave.saveData[idx];
                    // 변경
                    newData.state = EUIDState.Modify;

                    modify += $"\t{newData.objName}";
                    if (oldData.path.Equals(newData.path) == false)
                    {
                        var str = $"[PATH]: {oldData.path} -> {newData.path}";
                        modify += $"{_lineSkip}\t\t{str}";
                        newData.detail = str + " | ";
                    }
                    // [TODO] 변경된 부분을 old, new 컬럼으로 했으면 
                    if (oldData.uid.Equals(newData.uid) == false)
                    {
                        var str = $"[UID]: {oldData.uid} -> {newData.uid}";
                        modify += $"{_lineSkip}\t\t{str}";

                        newData.oldUID = oldData.uid;
                        newData.newUID = newData.uid;

                        newData.detail += str;
                    }
                    modify += _lineSkip;
                    oldData.isCheck = true;
                    modifyCnt++;
                }
            }

            var deleteList = oldSave.saveData.FindAll(x => x.isCheck == false);
            if (deleteList != null)
            {
                delete += $"[삭제 UID] - {deleteList.Count}";
            }

            result = $"[{title} COMPLETE CNT] - {newSave.saveData.Count}{_lineSkip}[ADD UID CNT] - {addCnt}{_lineSkip}\t{add}{_lineSkip}[MODITY UID CNT] - {modifyCnt}{_lineSkip}{modify}{_lineSkip}{delete}";
            SaveCsv(name, newSave);
            return result;
        }

#if false // Json
    private string SaveJson(string name, Save save)
    {
        SaveCsv(save);

        var newJson = new JsonFile<Save>();
        newJson.data = save;

        string result = "", modify = "", delete = "", add = "";
        int modifyCnt = 0, addCnt = 0;
        // 기존 파일 읽어서 비교
        var oldJson = new JsonFile<Save>();
        oldJson.LoadStreamingAssets($"{savePath}/{newJson.data.sceneName}.json", false);
        if(oldJson.data == null)
        {
            newJson.WriteStreamingAssets($"{savePath}/{newJson.data.sceneName}.json", false);
            return $"[UID 갯수] - {newJson.data.saveData.Count}";
        }
        oldJson.data.saveData.ForEach(data => { data.isCheck = false; });

        foreach (var newData in newJson.data.saveData)
        {
            var pathIdx = oldJson.data.saveData.FindIndex(x => x.path.Equals(newData.path));
            var uidIdx = oldJson.data.saveData.FindIndex(x => x.uid.Equals(newData.uid));
            var objNameIdx = oldJson.data.saveData.FindIndex(x => x.objName.Equals(newData.objName));

            if (pathIdx == uidIdx && objNameIdx == uidIdx)
            {
                if (pathIdx != -1)
                {
                    // 변경없음
                    Debug.Log($"[변경없음] Paht: [{newData.path}], UID: [{newData.path}]");
                    var oldData = oldJson.data.saveData[pathIdx];
                    oldData.isCheck = true;
                }
                else
                {
                    // 추가
                    var str = $"\t{newData.objName} \r\n\t\t- [Path]: {newData.path} \r\n\t\t- [UID]: {newData.uid}\r\n";
                    add += str;
                    addCnt++;
                }
            }
            else
            {
                var idx = Math.Max(Math.Max(pathIdx, uidIdx), objNameIdx);
                var oldData = oldJson.data.saveData[idx];
                // 변경
                var str = $"\t{newData.objName} \r\n\t\t- [Path]: {oldData.path} -> {newData.path} \r\n\t\t- [UID]: {oldData.uid} -> {newData.uid}\r\n";
                modify += str;
                oldData.isCheck = true;
                modifyCnt++;
            }
        }

        var deleteList =  oldJson.data.saveData.FindAll(x => x.isCheck == false);
        if(deleteList != null)
        {
            delete += $"[삭제 UID] - {deleteList.Count}";
        }
        result = $"[UID 갯수] - {newJson.data.saveData.Count}\r\n\r\n[추가 UID] - {addCnt}\r\n\t{add}\r\n[변경 UID] - {modifyCnt}\r\n{modify}\r\n{delete}";
        newJson.WriteStreamingAssets($"{savePath}/{newJson.data.sceneName}.json", false);
        return result;
    }
#endif

        /// <summary>
        /// substringToFind를 포함한 오브젝트이 이름 모두 찾기
        /// </summary>
        private List<T> FindObjectsByNameRecursive<T>(GameObject prefabValiant, string substringToFind) where T : Component
        {
            var allObjects = prefabValiant.GetComponentsInChildren<T>(true);
            return allObjects.Where(obj => obj.name.IndexOf(substringToFind, StringComparison.OrdinalIgnoreCase) >= 0)
                ?.ToList();
        }
        /// <summary>
        /// 게임 오브젝트의 Path (Root 포함)
        /// </summary>
        private string GetGameObjectPath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return string.Empty;
            }
            string path = gameObject.name;
            Transform current = gameObject.transform;

            while (current.parent != null)
            {
                current = current.parent;
                if (current.parent != null)
                {
                    path = current.name + "/" + path;
                    if (PrefabUtility.IsAnyPrefabInstanceRoot(current.gameObject) == true)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            return path;
        }
        private string GetGameObjectRootDeletePath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return string.Empty;
            }
            string path = gameObject.name;
            Transform current = gameObject.transform;

            while (current.parent != null)
            {
                current = current.parent;
                if (current.parent != null && PrefabUtility.IsAnyPrefabInstanceRoot(current.gameObject) == false)
                {
                    path = current.name + "/" + path;
                }
                else
                {
                    break;
                }
            }
            return path;
        }

        private string GetGameObjectRootPrefab(GameObject gameObject)
        {
            if (gameObject != null)
            {
                Transform current = gameObject.transform;
                while (current.parent != null)
                {
                    current = current.parent;
                    if (current.parent != null && PrefabUtility.IsAnyPrefabInstanceRoot(current.gameObject) == true)
                    {
                        return current.gameObject.name;
                    }
                }
            }
            return string.Empty;
        }
        /// <summary>
        ///  Variant(현재 씬)의 오브젝트를 Original Prefab에서 찾기
        /// </summary>
        private GameObject FindObjectOriPrefab(GameObject objVariant)
        {
            var variantOri = PrefabUtility.GetCorrespondingObjectFromOriginalSource(objVariant);
            return variantOri;
        }
        /// <summary>
        /// Original Prefab Object를 현재 씬에서 찾기
        /// </summary>
        private GameObject FindObjectAutoUIDScene(GameObject objOriPrefab)
        {
            var path = GetGameObjectRootDeletePath(objOriPrefab);

            // 현재 씬의 모든 오브젝트의 Prefab Root를 찾는다.
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            List<GameObject> findPrefabRoots = new List<GameObject>();
            foreach (var rootGameObject in rootGameObjects)
            {
                FindPrefabRoots(rootGameObject, findPrefabRoots);
            }
            // Prefab Root에서 Path를 이용해서 같은 오브젝트를 찾는다.
            if (TryFindPathObject(findPrefabRoots, path, out var findObject) == true)
            {
                Selection.activeGameObject = findObject;
            }
            return findObject;
        }

        private bool TryFindPathObject(List<GameObject> objRoots, string path, out GameObject retObj)
        {
            retObj = null;
            foreach (var root in objRoots)
            {
                retObj = root.transform.Find(path)?.gameObject;
                if (retObj != null)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 현재 오브젝트에서 부터 하위 모든 오브젝트까지 Prefab Root Object 찾는다.
        /// </summary>
        private void FindPrefabRoots(GameObject gameObject, List<GameObject> prefabsInScene)
        {
            // Prefab Root Object
            if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject) == true)
            {
                prefabsInScene.Add(gameObject);
            }

            // 재귀호출: 하위 오브젝트에서 찾는다
            foreach (Transform child in gameObject.transform)
            {
                FindPrefabRoots(child.gameObject, prefabsInScene);
            }
        }
        /// <summary>
        /// 현재 오브젝트 부모로 부터 prefixName을 찾는다.
        /// </summary>
        private string GetPrefixName(GameObject obj, string path)
        {
            var trn = obj.transform;
            var paths = path.Split('/');
            string prefixName = "";
            foreach (var p in paths)
            {
                if (trn == null)
                {
                    trn = obj.transform.Find(p);
                }
                else
                {
                    if (trn.TryGetComponent<RegAutoUID>(out var findAutoUID) == true)
                    {
                        prefixName += $"{findAutoUID._prefixName}_";
                    }
                    trn = trn.Find(p);
                }
            }

            if(dicPrefabByPrefixName.ContainsKey(obj.name) == true)
            {
                prefixName = dicPrefabByPrefixName[obj.name] + "_" +prefixName;
            }

            return prefixName;
        }
        /// <summary>
        /// 현재 씬의 Prefab Root Object를 모두 찾는다.
        /// </summary>
        private void FindPrefabsRecursive(GameObject gameObject, List<GameObject> prefabsInScene)
        {
            // Find Prefab Root Object
            if (PrefabUtility.IsPartOfPrefabInstance(gameObject) && PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
            {
                prefabsInScene.Add(gameObject);
            }
            // 재귀호출: 하위 오브젝트에서 찾는다
            foreach (Transform child in gameObject.transform)
            {
                FindPrefabsRecursive(child.gameObject, prefabsInScene);
            }
        }
#endif
    }
}