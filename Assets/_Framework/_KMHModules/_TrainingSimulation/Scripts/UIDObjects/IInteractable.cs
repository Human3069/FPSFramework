using Cysharp.Threading.Tasks;

namespace VTSFramework.TSModule
{
    public interface IInteractable
    {
        UniTask WaitUntilCorrectInteract(string _targetValue);
    }
}