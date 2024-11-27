using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace _KMH_Framework._TS_Module
{
    public class CsvFile
    {
        static public void WriteStreamingAssets(string sourcePath, Action<StreamWriter> retWR)
        {
            retWR?.Invoke(new StreamWriter($"{Application.streamingAssetsPath}/{sourcePath}.csv", false, Encoding.UTF8));
        }

        static public bool LoadStreamingAssets(string sourcePath, bool isExceptFirstLine = true, Action<string[]> retData = null)
        {
            FileInfo theSourceFile = new FileInfo($"{Application.streamingAssetsPath}/{sourcePath}.csv");
            if (theSourceFile != null && theSourceFile.Exists)
            {
                var sr = new StreamReader(theSourceFile.OpenRead(), Encoding.UTF8, false);
                string strLineValue = null;
                // 첫 라인은 타이틀로 설정. 라인 넘김
                if (isExceptFirstLine == true)
                {
                    sr.ReadLine();
                }

                while ((strLineValue = sr.ReadLine()) != null)
                {
                    retData?.Invoke(strLineValue.Split(','));
                }
                sr.Close();
                return true;
            }
            else
            {
                $"[FileIO] CSV LoadFile : sourcePath == NULL && referencePath == NULL".Log(LogType.Warning);
            }
            return false;
        }
    }
}