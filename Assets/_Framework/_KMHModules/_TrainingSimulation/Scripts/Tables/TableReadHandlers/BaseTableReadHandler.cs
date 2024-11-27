using Cysharp.Threading.Tasks;
using GoogleSheetsToUnity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public class SingleSheet<T> where T : IRow<T>
    {
        [SerializeField]
        internal string jsonName;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        internal List<T> allTableList = new List<T>();
        [ReadOnly]
        [SerializeField]
        internal List<T> sortedTableList = new List<T>();
    }

    public abstract class BaseTableReadHandler<T> : MonoBehaviour where T : IRow<T>
    {
        private const string LOG_FORMAT = "<color=white><b>[BaseTableReadHandler]</b></color> {0}";
        private const int SHEET_0 = 0;

        [Header("=== BaseTableReadHandler ===")]
        [ReadOnly]
        [SerializeField]
        protected bool _isJsonReadComplete = false;
        public bool IsJsonReadComplete
        {
            get
            {
                return _isJsonReadComplete;
            }
            set
            {
                _isJsonReadComplete = value;
            }
        }

        [Space(10)]
        [SerializeField]
        protected string jsonUri;

        [Space(10)]
        public List<SingleSheet<T>> SheetList = new List<SingleSheet<T>>();

        public virtual SingleSheet<T> FirstSheet
        {
            get
            {
                return SheetList[SHEET_0];
            }
        }

        public virtual async UniTask WaitUntilReady()
        {
            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return IsJsonReadComplete == true;
            }
        }

        public virtual void ReadExcel()
        {
            Debug.Assert(IsJsonReadComplete == false);

            foreach (SingleSheet<T> sheet in SheetList)
            {
                string _path = Application.streamingAssetsPath + "/" + jsonUri + "/" + sheet.jsonName;
                if (_path.Contains(".json") == false)
                {
                    _path += ".json";
                }
                string _json = File.ReadAllText(_path);

                sheet.allTableList = JsonConvert.DeserializeObject<List<T>>(_json);
                for (int i = 0; i < sheet.allTableList.Count; i++)
                {
                    sheet.allTableList[i] = sheet.allTableList[i].Validated();
                }
                sheet.sortedTableList = sheet.allTableList.FindAll(SortConditionPredicate);
            }

            IsJsonReadComplete = true;
        }

        protected abstract bool SortConditionPredicate(T row);
    }
}
