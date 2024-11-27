using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace _KMH_Framework
{
    [System.Serializable]
    public class ConfigurationHandler<T> where T : IConfig<T>
    {
        private const string LOG_FORMAT = "<color=white><b>[ConfigurationHandler]</b></color> {0}";

        public string path;
        [ReadOnly]
        public T Result;

        public void Read()
        {
            if (path.Contains(".xml") == false)
            {
                path += ".xml";
            }

            string fullPath = Application.streamingAssetsPath + "/" + path;
            if (File.Exists(fullPath) == false)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "fullPath : " + fullPath + " Not Exist File!");
                return;
            }

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StreamReader _streamReader = new StreamReader(fullPath);

            Result = (T)xmlSerializer.Deserialize(_streamReader.BaseStream);
            Result = Result.Parsed();

            _streamReader.Close();
        }
    }
}