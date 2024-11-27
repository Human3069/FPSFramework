using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace VTSFramework.TSModule
{
    public class _2DButton : _2DInteractable
    {
        [Header("=== _2DButton ===")]
        public UnityEvent OnClickDownEvent;
        public UnityEvent OnClickUpCompletedEvent;
        public UnityEvent OnClickUpCancelledEvent;

        protected bool isClickedOnWait = false;

        public override async UniTask WaitUntilCorrectInteract(string _targetValue)
        {
            isClickedOnWait = false;

            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return isClickedOnWait == true;
            }

            IsHover = false;
        }

        protected override void OnClickDown()
        {
            OnClickDownEvent.Invoke();
        }

        protected override void OnClickUpCompletely()
        {
            OnClickUpCompletedEvent.Invoke();
            isClickedOnWait = true;
        }

        protected override void OnClickUpCancelled()
        {
            OnClickUpCancelledEvent.Invoke();
        }
    }
}