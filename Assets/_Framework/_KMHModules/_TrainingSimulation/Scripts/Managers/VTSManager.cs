using _KMH_Framework;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class VTSManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[VTSManager]</b></color> {0}";

        protected static VTSManager _instance;
        public static VTSManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        [Header("Tables")]
        public ScenarioTableReadHandler ScenarioTable;
        public PopupTableReadHandler PopupTable;
        public LocalizeTableReadHandler LocalizeTable;
        public MissionSettingTableReadHandler MissionSettingTable;
        public TaskSettingTableReadHandler TaskSettingTable;

        public enum MissionType
        {
            m_0_0,
            m_0_1,
            m_0_2
        }

        public Enum _MissionType;

        public enum LanguageType
        {
            korean,
            english
        }

        public Enum _LanguageType;

        protected float _timeScaleValue;
        protected float TimeScaleValue
        {
            get
            {
                return _timeScaleValue;
            }
            set
            {
                if (_timeScaleValue != Mathf.Clamp(value, 0f, 8f))
                {
                    _timeScaleValue = Mathf.Clamp(value, 0f, 8f);
                    Time.timeScale = _timeScaleValue;

                    Debug.LogFormat(LOG_FORMAT, "배속 변경 x" + _timeScaleValue);
                }
            }
        }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogErrorFormat(LOG_FORMAT, "");
                Destroy(this.gameObject);
                return;
            }

            PostAwake().Forget();

#if UNITY_EDITOR
            bool isEnumDirty = false;
            foreach (string enumValue in Enum.GetNames(typeof(MissionType)))
            {
                if (enumValue.Equals(enumValue.ToInMissionText()) == false)
                {
                    isEnumDirty = true;
                    break;
                }
            }

            foreach (string enumValue in Enum.GetNames(typeof(LanguageType)))
            {
                if (enumValue.Equals(enumValue.ToInMissionText()) == false)
                {
                    isEnumDirty = true;
                    break;
                }
            }

            if (isEnumDirty == true)
            {
                Debug.LogErrorFormat(LOG_FORMAT, "enum 값에 대문자가 포함될 수 없습니다.");
            }
#endif
        }

        protected virtual async UniTaskVoid PostAwake()
        {
            await UniTask.WaitWhile(() => ConfigurationReader.Instance == null);
            await ConfigurationReader.Instance.WaitUntilReady();

            if (ConfigurationReader.Instance.UserConfigHandler.Result.IsUseCheat == true)
            {
                while (true)
                {
                    if (Input.GetKeyDown(KeyCode.Insert) == true)
                    {
                        TimeScaleValue++;
                    }
                    else if (Input.GetKeyDown(KeyCode.Delete) == true)
                    {
                        TimeScaleValue--;
                    }

                    await UniTask.NextFrame();
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }
    }
}