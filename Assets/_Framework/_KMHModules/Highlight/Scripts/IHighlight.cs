using Cysharp.Threading.Tasks;

namespace VTSFramework.TSModule
{
    public interface IHighlight : ITypable
    {
        public float HighlightDuration
        {
            get;
            set;
        }

        public UniTaskVoid DisableAsync();
    }
}