using UnityEngine;

namespace VTSFramework.TSModule
{
    [System.Serializable]
    public struct LocalizeRow : IRow<LocalizeRow>
    {
        public LocalizeRow Validated()
        {
            return this;
        }

        [ReadOnly]
        public int Table_Id;

        [Space(10)]
        [ReadOnly]
        public string Korean_String;
        [ReadOnly]
        public string English_String;
    }
}