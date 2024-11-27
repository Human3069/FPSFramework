using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace _KMH_Framework._TS_Module
{
    // [fov-studio update] 2024.04
    [SerializeField]
    public abstract class _ABaseTable
    {
        public abstract List<S> GetDataList<S>();

        /// <summary>
        /// [_BaseTable] <para/>
        /// - 생성자 인자를 Path("[path]/[sheetName].json")으로 json파일을 데이타 로드한다. <para/>
        /// * <para/>
        /// [ _BaseTableMerge] <para/>
        /// - SheetName의 테이블 데이타를 병합한다. <para/>
        /// - Dictionary의 Key Value로 SheetName이 사용되며 삭제할때도 사용된다.<para/>
        /// </summary>
        public abstract void LoadFile();

        /// <summary>
        /// [_BaseTable] <para/>
        /// - 생성자 인자를 Path("[path]/[sheetName].json")으로 json파일을 데이타 로드 <para/>
        /// * <para/>
        /// [_BaseTableMerge] <para/>
        /// - fileName의 테이블 데이타를 병합한다.<para/>
        /// - Dictionary의 Key Value로 fileName이 사용되며 삭제할때도 사용된다.<para/>
        /// </summary
        public abstract void LoadFile(string fileName);

        /// <summary>
        /// [_BaseTableMerge] <para/>
        /// SheetName을 Key값으로 데이타를 찾아서 삭제한다.
        /// </summary>
        public virtual void RmoveTable(string filename) { }

        /// <summary>
        /// TableData에서 특정 데이타를 찾아주는 함수를 각 각 구현 할 것
        /// </summary>
        public Func<object, object> FindTableDataCallback;

        protected string strSheetName = "";
        public string SheetName
        {
            get => strSheetName;
            set => strSheetName = value;
        }

        private string path = "Assets";
        public string Path
        {
            get => path;
            set => path = value;
        }

        private bool isDownload = false;
        public bool IsDownload
        {
            get => isDownload;
            set => isDownload = value;
        }

        public T GetTableType<T>() where T : Enum
        {
            return (T)eTable;
        }

        public void SetTableType(Enum tableType)
        {
            eTable = tableType;
        }

        private Enum eTable;
        public Enum SheetType
        {
            set => eTable = value;
        }
    }

    // [fov-studio update] 2024.04
    public class _BaseTable<T> : _ABaseTable
    {
        public List<T> data = new List<T>();

        public _BaseTable()
        {
        }
        public _BaseTable(string path, string filename, Enum sheetType)
        {
            ResetSheetData(path, filename);
            SetTableType(sheetType);
        }
        public void ResetSheetData(string path, string filename)
        {
            SheetName = filename;
            Path = path;
        }
        public override List<S> GetDataList<S>()
        {
            return data as List<S>;
        }

        public void LoadAsset(AssetBundle asset, string _name)
        {
            var json = asset.LoadAsset<TextAsset>(_name).text;
            data = JsonConvert.DeserializeObject<List<T>>(json);
        }

        public override void LoadFile(string filename)
        {
            var sr = new StreamReader(filename);
            var json = sr.ReadToEnd();
            data = JsonConvert.DeserializeObject<List<T>>(json);
            sr.Close();
        }

        public override void LoadFile()
        {
            var sr = new StreamReader($"{Path}/{SheetName}.json");
            var json = sr.ReadToEnd();
            data = JsonConvert.DeserializeObject<List<T>>(json);
            sr.Close();
        }
    }

}