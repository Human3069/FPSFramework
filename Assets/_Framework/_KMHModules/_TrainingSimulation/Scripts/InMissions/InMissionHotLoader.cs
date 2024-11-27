using _KMH_Framework;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    // 핫로드 전용
    public class InMissionHotLoader : MonoBehaviour
    {
        [Header("HotLoad")]
        [SerializeField]
        protected VTSManager.LanguageType languageType;
        [SerializeField]
        protected VTSManager.MissionType missionType;

        [SerializeField]
        protected Enum languageTypeE;

        protected UniTask.Awaiter awater;

        protected virtual void Awake()
        {
            awater = HotLoad().GetAwaiter();
        }

        // 핫로드 순서 :
        // 1. Config.xml 파싱
        // 2. 로컬라이즈 테이블 파싱
        // 3. 시나리오 테이블 파싱
        // 4. 팝업 테이블 파싱
        // 5. 미션 세팅 테이블 파싱
        // 6. 태스크 세팅 테이블 파싱

        // 7. 초기화
        // 8. UID 등록

        protected virtual async UniTask HotLoad()
        {
            await UniTask.WaitWhile(PredicateFunc);
            bool PredicateFunc()
            {
                bool isInMissionHandlerNotLoaded = InMissionHandler.Instance == null;
                bool isVTSManagerNotLoaded = VTSManager.Instance == null;

                return isInMissionHandlerNotLoaded == true || isVTSManagerNotLoaded == true;
            }

            VTSManager.Instance._MissionType = missionType;
            VTSManager.Instance._LanguageType = languageType;

            await ConfigurationReader.Instance.WaitUntilReady();

            LocalizeTableReadHandler localize = VTSManager.Instance.LocalizeTable;
            localize.ReadExcel();
            await localize.WaitUntilReady();

            ScenarioTableReadHandler scenario = VTSManager.Instance.ScenarioTable;
            scenario.ReadExcel();
            await scenario.WaitUntilReady();

            PopupTableReadHandler popup = VTSManager.Instance.PopupTable;
            popup.ReadExcel();
            await popup.WaitUntilReady();

            MissionSettingTableReadHandler missionSetting = VTSManager.Instance.MissionSettingTable;
            missionSetting.ReadExcel();
            await missionSetting.WaitUntilReady();

            TaskSettingTableReadHandler taskSetting = VTSManager.Instance.TaskSettingTable;
            taskSetting.ReadExcel();
            await taskSetting.WaitUntilReady();

            InMissionHandler.Instance.InMissionUi.Initialize();

            await UIDManager.Instance.RegisterAllUidObjects();
        }

        public virtual async UniTask WaitUntilReady()
        {
            await UniTask.WaitUntil(PredicateFunc);
            bool PredicateFunc()
            {
                return awater.IsCompleted == true;
            }
        }
    }
}