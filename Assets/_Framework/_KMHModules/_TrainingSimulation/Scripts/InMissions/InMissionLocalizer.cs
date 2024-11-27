using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class InMissionLocalizer : MonoBehaviour
    {
#if false
        private const string LOG_FORMAT = "<color=white><b>[InMissionLocalizer]</b></color> {0}";

        protected static InMissionLocalizer _instance;
        public static InMissionLocalizer Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        public enum SheetType
        {
            inMission,
            outMission,
            popup
        }

        [ReadOnly]
        [SerializeField]
        [SerializedDictionary("Table ID", "Text")]
        protected SerializedDictionary<int, string> inMissionTextDic = new SerializedDictionary<int, string>();
        [ReadOnly]
        [SerializeField]
        [SerializedDictionary("Table ID", "Text")]
        protected SerializedDictionary<int, string> outMissionTextDic = new SerializedDictionary<int, string>();
        [ReadOnly]
        [SerializeField]
        [SerializedDictionary("Table ID", "Text")]
        protected SerializedDictionary<int, string> popupTextDic = new SerializedDictionary<int, string>();

        // 시트 추가 및 삭제할 때마다 코드를 수정하지 않게 하기위해 추가됨.
        protected List<SerializedDictionary<int, string>> textDicList = new List<SerializedDictionary<int, string>>();
        protected List<LocalizeTableReadHandler> localizeHandlerList = new List<LocalizeTableReadHandler>();

        protected List<FieldInfo> localizerDicFieldInfoList = new List<FieldInfo>();

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this);
                return;
            }

            FieldInfo[] managerFieldInfo = typeof(VTSManager).GetDeclaredFields();
            localizeHandlerList = new List<LocalizeTableReadHandler>();
            foreach (FieldInfo info in managerFieldInfo)
            {
                object baseField = info.GetValue(VTSManager.Instance);
                if (baseField is LocalizeTableReadHandler)
                {
                    localizeHandlerList.Add(baseField as LocalizeTableReadHandler);
                }
            }

            FieldInfo[] localizerFieldInfos = this.GetType().GetDeclaredFields();
            foreach (FieldInfo info in localizerFieldInfos)
            {
                object baseField = info.GetValue(this);
                if (baseField is SerializedDictionary<int, string>)
                {
                    localizerDicFieldInfoList.Add(info);
                }
            }

            textDicList = new List<SerializedDictionary<int, string>>();
            foreach (FieldInfo info in localizerDicFieldInfoList)
            {
                object baseField = info.GetValue(this);
                textDicList.Add(baseField as SerializedDictionary<int, string>);
            }

            Debug.Assert(localizeHandlerList.Count == textDicList.Count, "localizeHandlerList.Count : " + localizeHandlerList.Count + ", textDicList.Count : " + textDicList.Count);
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        public virtual void LoadLocalizedTexts()
        {
            VTSManager manager = VTSManager.Instance;
            string languageType = manager._LanguageType.ToString();

            for (int i = 0; i < localizeHandlerList.Count; i++)
            {
                foreach (LocalizeRow row in localizeHandlerList[i].SortedTableList)
                {
                    FieldInfo[] infos = typeof(LocalizeRow).GetDeclaredFields();
                    string text = "UNDEFINED!!!";
                    foreach (FieldInfo info in infos)
                    {
                        if (info.Name.ToLower().Contains(languageType) == true)
                        {
                            text = info.GetValue(row) as string;
                            break;
                        }
                    }

                    textDicList[i].Add(row.Table_Id, text);
                }
            }
        }

        public virtual string GetLocalizedString(SheetType sheetType, int tableId)
        {
            SerializedDictionary<int, string> targetDic = null;
            foreach (FieldInfo info in localizerDicFieldInfoList)
            {
                if (info.Name.Contains(sheetType.ToString()) == true)
                {
                    targetDic = info.GetValue(this) as SerializedDictionary<int, string>;
                }
            }

            if (targetDic.ContainsKey(tableId) == true)
            {
                return targetDic[tableId];
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, tableId + "는 Localize 테이블의 " + sheetType + " 시트에 존재하지 않는 번호입니다.");
                return "UNKNOWN VALUE !!";
            }
        }
#endif
    }
}