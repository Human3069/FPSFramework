namespace VTSFramework.TSModule
{
    public interface ITypable
    {
        public InMissionType _InMissionType
        {
            get;
            set;
        }

        public bool enabled
        {
            get;
            set;
        }
    }
}