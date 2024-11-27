using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace VTSFramework.TSModule
{
    [RequireComponent(typeof(Collider))]
    public class _3DButton : _3DInteractable
    {
        [Header("=== _3DButton ===")]
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
        }

        public override void OnRegistered(UIDObject uidObj)
        {
            base.OnRegistered(uidObj);

            _collider = this.GetComponent<Collider>();
        }

        public override void OnClickDown()
        {
            OnClickDownEvent.Invoke();
        }

        public override void OnClickUpCompletely()
        {
            OnClickUpCompletedEvent.Invoke();
            isClickedOnWait = true;
        }

        public override void OnClickUpCancelled()
        {
            OnClickUpCancelledEvent.Invoke();
        }
    }
}