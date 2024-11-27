using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class _2DInputField : _2DInteractable
    {
        protected TMP_InputField inputField;

        public Action<bool> OnSelectAction;

        protected override void Awake()
        {
            base.Awake();

            inputField = this.GetComponent<TMP_InputField>();
            inputField.onSelect.AddListener(OnSelect);
            void OnSelect(string text)
            {
                if (IsCurrentInteractable == true)
                {
                    OnSelectAction.Invoke(true);
                }
                else
                {
                    inputField.ReleaseSelection();
                    inputField.DeactivateInputField();
                }
            }

            inputField.onDeselect.AddListener(OnDeselect);
            void OnDeselect(string text)
            {
                OnSelectAction.Invoke(false);
            }

            inputField.interactable = false;
        }

        public override async UniTask WaitUntilCorrectInteract(string _targetValue)
        {
            inputField.interactable = true;

            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return _targetValue.Equals(inputField.text) == true &&
                       inputField.isFocused == false;
            }

            inputField.interactable = false;
        }

        protected override void OnClickDown()
        {
            //
        }

        protected override void OnClickUpCancelled()
        {
            //
        }

        protected override void OnClickUpCompletely()
        {
            //
        }
    }
}