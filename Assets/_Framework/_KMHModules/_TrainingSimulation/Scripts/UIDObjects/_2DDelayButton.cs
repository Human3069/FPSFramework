using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utilities;

namespace VTSFramework.TSModule
{
    public class _2DDelayButton : _2DInteractable
    {
        public enum DelayType
        {
            None,
            Fixed,
            Continuous
        }

        [Header("=== _2DDelayButton ===")]
        [SerializeField]
        protected DelayType delayType;
        [SerializeField]
        protected float duration = 2f;
        [ReadOnly]
        [SerializeField]
        protected float _currentDuration;
        protected float CurrentDuration
        {
            get
            {
                return _currentDuration;
            }
            set
            {
                _currentDuration = value;

                if (delayType == DelayType.Fixed)
                {
                    for (int i = 0; i < targetFixedTimeList.Count; i++)
                    {
                        onObjs[i].SetActive(targetFixedTimeList[i] < value);
                    }
                }
                else if (delayType == DelayType.Continuous)
                {
                    image.fillAmount = value / duration;
                }
                else
                {
                    //
                }

                OnValueChangedEvent.Invoke(value);
            }
        }

        [Space(10)]
        [SerializeField]
        public UnityEvent OnClickDownEvent;
        public UnityEvent<float> OnClickUpCancelledEvent; // 정해진 시간을 채우지 않고 뗐을 때
        public UnityEvent OnClickUpCompletelyEvent; // 정해진 시간을 다 채우고 뗐을 때
        public UnityEvent<float> OnValueChangedEvent;

        [Space(10)]
        [SerializeField]
        protected GameObject[] onObjs;
        [Space(10)]
        [SerializeField]
        protected Image image;

        protected List<float> targetFixedTimeList = new List<float>();
        protected bool isClickedOnWait = false;

        protected override void Awake()
        {
            base.Awake();

            if (delayType == DelayType.Fixed)
            {
                Debug.Assert(onObjs.Length != 0);
                for (int i = 0; i < onObjs.Length; i++)
                {
                    float targetTime = duration * i / (float)onObjs.Length;
                    targetFixedTimeList.Add(targetTime);
                }
            }
            else if (delayType == DelayType.Continuous)
            {
                Debug.Assert(image != null);
            }

            CurrentDuration = 0f;
        }

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
            PostOnClickDown().Forget();
        }

        protected virtual async UniTaskVoid PostOnClickDown()
        {
            OnClickDownEvent.Invoke();
            CurrentDuration = 0f;

            while (CurrentDuration < duration)
            {
                await UniTaskEx.NextFrame(this, 0);
                CurrentDuration = Mathf.MoveTowards(CurrentDuration, duration, Time.deltaTime);
            }

            isClickedOnWait = true;
            OnClickUpCompletelyEvent.Invoke();
            CurrentDuration = 0f;
        }

        protected override void OnClickUpCancelled()
        {
            if (CurrentDuration != duration)
            {
                OnClickUpCancelledEvent.Invoke(CurrentDuration);
            }

            UniTaskEx.Cancel(this, 0);
            CurrentDuration = 0f;
        }

        protected override void OnClickUpCompletely()
        {
            if (CurrentDuration != duration)
            {
                OnClickUpCancelledEvent.Invoke(CurrentDuration);
            }

            UniTaskEx.Cancel(this, 0);
            CurrentDuration = 0f;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(_2DDelayButton))]
    [CanEditMultipleObjects]
    public class _2DDelayButtonEditor : Editor
    {
        protected SerializedProperty m_Script;

        protected SerializedProperty _isInteractable;
        protected SerializedProperty _isInteractableWithoutInMission;
        protected SerializedProperty hasFx;
        protected SerializedProperty interactionFx;
        protected SerializedProperty OnHoverEvent;

        protected SerializedProperty delayType;
        protected SerializedProperty duration;
        protected SerializedProperty _currentDuration;

        protected SerializedProperty OnClickDownEvent;
        protected SerializedProperty OnClickUpCancelledEvent; 
        protected SerializedProperty OnClickUpCompletelyEvent;
        protected SerializedProperty OnValueChangedEvent;

        protected SerializedProperty onObjs;
        protected SerializedProperty image;

        protected virtual void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");

            _isInteractable = serializedObject.FindProperty("_isInteractable");
            _isInteractableWithoutInMission = serializedObject.FindProperty("_isInteractableWithoutInMission");
            hasFx = serializedObject.FindProperty("hasFx");
            interactionFx = serializedObject.FindProperty("interactionFx");
            OnHoverEvent = serializedObject.FindProperty("OnHoverEvent");

            delayType = serializedObject.FindProperty("delayType");
            duration = serializedObject.FindProperty("duration");
            _currentDuration = serializedObject.FindProperty("_currentDuration");

            OnClickDownEvent = serializedObject.FindProperty("OnClickDownEvent");
            OnClickUpCancelledEvent = serializedObject.FindProperty("OnClickUpCancelledEvent");
            OnClickUpCompletelyEvent = serializedObject.FindProperty("OnClickUpCompletelyEvent");
            OnValueChangedEvent = serializedObject.FindProperty("OnValueChangedEvent");

            onObjs = serializedObject.FindProperty("onObjs");
            image = serializedObject.FindProperty("image");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayoutEx.PropertyFieldWithDisabled(m_Script);

            EditorGUILayout.PropertyField(_isInteractable);
            EditorGUILayout.PropertyField(_isInteractableWithoutInMission);
            EditorGUILayout.PropertyField(hasFx);
            EditorGUILayout.PropertyField(interactionFx);
            EditorGUILayout.PropertyField(OnHoverEvent);

            EditorGUILayout.PropertyField(delayType);
            EditorGUILayout.PropertyField(duration);
            EditorGUILayout.PropertyField(_currentDuration);

            EditorGUILayout.PropertyField(OnClickDownEvent);
            EditorGUILayout.PropertyField(OnClickUpCancelledEvent);
            EditorGUILayout.PropertyField(OnClickUpCompletelyEvent);
            EditorGUILayout.PropertyField(OnValueChangedEvent);

            if (delayType.GetValue<_2DDelayButton.DelayType>() == _2DDelayButton.DelayType.Fixed)
            {
                EditorGUILayout.PropertyField(onObjs);
            }

            if (delayType.GetValue<_2DDelayButton.DelayType>() == _2DDelayButton.DelayType.Continuous)
            {
                EditorGUILayout.PropertyField(image);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}