using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;


namespace _KMH_Framework._TS_Module
{
    /// <summary>
    /// [fov-studio update] 2024.05 <para/>
    /// 한 테이블에 같은 형식의 데이타 여러개를 합쳐서 사용할때 사용 <para/>
    /// 예) 공통 스트링 테이블 + 각 장치별 스트링 테이블
    /// </summary>
    public class _BaseTableMerge<T> : _BaseTable<T>
    {
        private Dictionary<string, List<T>> mergeDataByList = new();

        public _BaseTableMerge(string path, string sheetname, Enum sheetType) : base(path, sheetname, sheetType)
        {
        }

        public override void LoadFile(string fileName)
        {
            SetMerge(fileName);
        }

        public override void LoadFile()
        {
            SetMerge(SheetName);
        }

        private void SetMerge(string fileName)
        {
            if (mergeDataByList.ContainsKey(fileName) == false)
            {
                var sr = new StreamReader($"{Path}/{fileName}.json");
                var json = sr.ReadToEnd();

                var mergeData = JsonConvert.DeserializeObject<List<T>>(json);
                mergeDataByList.Add(fileName, mergeData);
                data.AddRange(mergeData);

                data = data.Distinct().ToList();
                sr.Close();
            }
        }

        public override void RmoveTable(string filename)
        {
            if (mergeDataByList.TryGetValue(filename, out var removeData) == true)
            {
                data = data.Except(removeData).ToList();
                mergeDataByList.Remove(filename);
                removeData.Clear();
            }
        }
    }
}