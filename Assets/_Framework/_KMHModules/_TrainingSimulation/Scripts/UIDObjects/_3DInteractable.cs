using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VTSFramework.TSModule
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public abstract class _3DInteractable : BaseObject, IInteractable
    {
        public static List<_3DInteractable> HoverList = new List<_3DInteractable>();
        protected Collider _collider;

        [Header("=== _3DInteractable ===")]
        [SerializeField]
        protected bool _isInteractable = true;
        public bool IsInteractable
        {
            get
            {
                return _isInteractable;
            }
        }

        [SerializeField]
        protected bool _isInteractableWithoutInMission = false; // true => 테이블 상관없이 인터렉션 가능함, false => 테이블에 따라 인터렉션 가능함
        public bool IsInteractableWithoutInMission
        {
            get
            {
                return _isInteractableWithoutInMission;
            }
        }

        [Space(10)]
        [SerializeField]
        protected bool hasFx;
        [DrawIf("hasFx", true)]
        [SerializeField]
        protected _3DInteractionFx interactionFx;

        [Header("=== _3DInteractable ===")]
        public UnityEvent<bool> OnHoverEvent;

        protected bool _isHover;
        public bool IsHover
        {
            get
            {
                return _isHover;
            }
            set
            {
                if (_isHover != value)
                {
                    _isHover = value;
                    OnHoverEvent.Invoke(value);
                }
            }
        }

        public abstract UniTask WaitUntilCorrectInteract(string _targetValue);

        protected virtual void Awake()
        {
            if (hasFx == true)
            {
                interactionFx.Initialize(this).Forget();
            }
        }

        public override void OnRegistered(UIDObject uidObj)
        {
            base.OnRegistered(uidObj);

            _collider = this.GetComponent<Collider>();
        }

        public abstract void OnClickDown();

        // OnClickDown => OnClickUp 둘다 Raycast Hit 상태일 때
        public abstract void OnClickUpCompletely();

        // OnClickDown만 Raycast Hit => OnClickUp 때 Hit이 아닐 때
        public abstract void OnClickUpCancelled();
    }
}