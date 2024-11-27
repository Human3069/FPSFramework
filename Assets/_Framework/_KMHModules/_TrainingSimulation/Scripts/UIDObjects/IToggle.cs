
namespace VTSFramework.TSModule
{
    public interface IToggle
    {
        bool IsOn
        {
            get;
            set;
        }

        void SetWithoutNotify(bool isOn);

        void SetSetting(string order);
    }
}