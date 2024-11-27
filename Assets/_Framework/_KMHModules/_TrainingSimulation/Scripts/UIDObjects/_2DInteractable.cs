using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace VTSFramework.TSModule
{
    public abstract class _2DInteractable : BaseObject, IInteractable, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private const string LOG_FORMAT = "<color=white><b>[_2DInteractable]</b></color> {0}";

        public static List<_2DInteractable> HoverList = new List<_2DInteractable>();

        [Header("=== _2DInteractable ===")]
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
        protected bool hasFx = true;
        [DrawIf("hasFx", true)]
        [SerializeField]
        protected _2DInteractionFx interactionFx;

        public UnityEvent<bool> OnHoverEvent;

        protected bool _isHover;
        protected bool IsHover
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

        protected virtual bool IsCurrentInteractable
        {
            get
            {
                bool isInteractable;

                if (UidObj == null)
                {
                    isInteractable = false;
                }
                else if (InteractionInMission.TargetUidList.Contains(UidObj.UidValue) == true)
                {
                    isInteractable = IsInteractable == true;
                }
                else
                {
                    isInteractable = IsInteractable == true &&
                                     IsInteractableWithoutInMission == true;
                }

                return isInteractable;
            }
        }

        protected virtual void Awake()
        {
            if (hasFx == true)
            {
                interactionFx.Initialize(this);
            }
        }

        public abstract UniTask WaitUntilCorrectInteract(string _targetValue);

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (IsCurrentInteractable == true)
            {
                IsHover = true;
                if (HoverList.Contains(this) == false)
                {
                    HoverList.Add(this);
                }
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            IsHover = false;
            if (HoverList.Contains(this) == true)
            {
                HoverList.Remove(this);
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (HoverList.Contains(this) == true)
            {
                OnClickDown();
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (IsCurrentInteractable == true)
            {
                if (HoverList.Contains(this) == true)
                {
                    OnClickUpCompletely();
                }
                else
                {
                    OnClickUpCancelled();
                }
            }
        }

        protected abstract void OnClickDown();

        protected abstract void OnClickUpCompletely();

        protected abstract void OnClickUpCancelled();
    }
}