using _KMH_Framework;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace VTSFramework.TSModule
{
    public class InMissionHandler : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[InMissionHandler]</b></color> {0}";

        protected static InMissionHandler _instance;
        public static InMissionHandler Instance
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

        protected static InMissionData _startData;
        public static InMissionData StartData
        {
            get
            {
                if (_startData == null)
                {
                    _startData = Resources.Load<InMissionData>("ScriptableObjects/InMissionData");
                }

                return _startData;
            }
        }

        [Header("Components")]
        public UI_MainInMission InMissionUi;

        protected InMissionHotLoader hotLoader;
        protected InMissionRecorder recorder;

        [Header("Information")]
        [ReadOnly]
        [SerializeField]
        protected int _firstIndex = -1;
        public int FirstIndex
        {
            get
            {
                return _firstIndex;
            }
            protected set
            {
                _firstIndex = value;
            }
        }

        [ReadOnly]
        [SerializeField]
        protected int _secondIndex = -1;
        public int SecondIndex
        {
            get
            {
                return _secondIndex;
            }
            protected set
            {
                _secondIndex = value;
            }
        }

        protected Dictionary<string, BaseInMission> inMissionDic = new Dictionary<string, BaseInMission>();

        [HideInInspector]
        public List<TaskRow> CurrentTaskRowList;

        public delegate void BeforeInMissionStepChange(int firstIndex, int secondIndex);
        public static event BeforeInMissionStepChange OnBeforeInMissionStepChange;

        protected virtual void Invoke_OnBeforeInMissionStepChange()
        {
            if (OnBeforeInMissionStepChange != null)
            {
                OnBeforeInMissionStepChange(FirstIndex, SecondIndex);
            }
        }

        public delegate void TaskChanged(List<TaskRow> taskRowList);
        public static event TaskChanged OnTaskChanged;

        protected virtual void Invoke_OnTaskChanged()
        {
            if (OnTaskChanged != null)
            {
                OnTaskChanged(CurrentTaskRowList);
            }
        }


        protected bool _isNextButtonClickable = false;
        public bool IsNextButtonClickable
        {
            get
            {
                return _isNextButtonClickable;
            }
            protected set
            {
                _isNextButtonClickable = value;

                Invoke_OnIsNextButtonClickableValueChanged();
            }
        }

        public delegate void IsNextButtonClickableValueChanged(bool value);
        public static event IsNextButtonClickableValueChanged OnIsNextButtonClickableValueChanged;

        protected virtual void Invoke_OnIsNextButtonClickableValueChanged()
        {
            if (OnIsNextButtonClickableValueChanged != null)
            {
                OnIsNextButtonClickableValueChanged(IsNextButtonClickable);
            }
        }

        #region Shortcut References...
        protected ScenarioTableReadHandler scenarioTable;
        protected TaskSettingTableReadHandler taskSettingTable;
        #endregion

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

            hotLoader = this.GetComponent<InMissionHotLoader>();
            recorder = this.GetComponent<InMissionRecorder>();
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void Start()
        {
            StartAsync().Forget();
        }

        // 순서
        // 1. 핫로더 로드
        // 2. 세팅테이블 (미션세팅)
        // 3. 세팅테이블 외의 초기세팅
        // 4. 리코더에 모든 진행상황 기록

        protected virtual async UniTaskVoid StartAsync()
        {
            // 컴포넌트들에서 Task 갖고오기
            BaseInMission[] baseInMissions = this.transform.GetComponentsInChildren<BaseInMission>();
            for (int i = 0; i < baseInMissions.Length; i++)
            {
                string direct = baseInMissions[i].Direct;
                string[] directs = direct.Split("||");

                for (int j = 0; j < directs.Length; j++)
                {
                    BaseInMission inMission = baseInMissions[i];
                    inMissionDic.Add(directs[j], inMission);
                    inMissionDic.Add(directs[j] + "auto", inMission);
                    inMissionDic.Add(directs[j] + "manual", inMission);
                }
            }

            // 필요한 부분들 Add
            scenarioTable = VTSManager.Instance.ScenarioTable;
            taskSettingTable = VTSManager.Instance.TaskSettingTable;

            await hotLoader.WaitUntilReady();

            VTSManager.Instance.MissionSettingTable.SetMissionSetting();
            StartData.MoveCameraToStartPoint();
            recorder.Initialize();

            FirstIndex = 0;
            SecondIndex = 0;

            RunAsync().Forget();
        }

        protected virtual async UniTaskVoid RunAsync()
        {
            IsNextButtonClickable = false;
            float delayBetweenStep = ConfigurationReader.Instance.UserConfigHandler.Result.DelayBetweenStep;

            while (true)
            {
                scenarioTable.InitializeTask(FirstIndex, SecondIndex);
                taskSettingTable.SetTaskSetting(FirstIndex, SecondIndex);

                CurrentTaskRowList = scenarioTable.GetAllSecondIndexList();
                Invoke_OnTaskChanged();

                List<UniTask.Awaiter> awaiterList = new List<UniTask.Awaiter>();

                foreach (TaskRow row in CurrentTaskRowList)
                {
                    if (inMissionDic.ContainsKey(row.Direct) == true)
                    {
                        if (string.IsNullOrEmpty(row.Parameter) == true)
                        {
                            Debug.LogFormat(LOG_FORMAT, "<color=yellow>명령어 [" + row.Direct + "] 실행</color>");
                        }
                        else
                        {
                            Debug.LogFormat(LOG_FORMAT, "<color=yellow>명령어 [" + row.Direct + "] 실행, 파라미터 [" + row.Parameter + "]</color>");
                        }

                        UniTask task = inMissionDic[row.Direct].DoInMissionAsync(row);
                        awaiterList.Add(task.GetAwaiter());
                    }
                    else
                    {
                        Debug.LogErrorFormat(LOG_FORMAT, "알 수 없는 명령어 [" + row.Direct + "] 발견");
                    }
                }

                foreach(UniTask.Awaiter awaiter in awaiterList)
                {
                    await UniTaskEx.WaitUntil(this, 0, PredicateFunc);
                    bool PredicateFunc()
                    {
                        return awaiter.IsCompleted == true;
                    }
                }

                await UniTaskEx.WaitForSeconds(this, 0, delayBetweenStep);

                if (scenarioTable.HasNextSecondIndex == true)
                {
                    SecondIndex++;
                }
                else
                {
                    break;
                }
            }

            IsNextButtonClickable = true;
        }

        public virtual void RunNextStep()
        {
            Invoke_OnBeforeInMissionStepChange();

            if (scenarioTable.HasNextFirstIndex == true)
            {
                FirstIndex++;
                SecondIndex = 0;

                RunAsync().Forget();
            }
            else
            {
                Debug.LogFormat(LOG_FORMAT, "<color=red><b>시나리오 종료</b></color>");
                IsNextButtonClickable = false;
            }
        }

        public virtual void RunPrev()
        {
            UniTaskEx.Cancel(this, 0);
            Invoke_OnBeforeInMissionStepChange();

            recorder.ReadRecord(FirstIndex, SecondIndex, false, out int targetFirstIndex, out int targetSecondIndex);
            FirstIndex = targetFirstIndex;
            SecondIndex = targetSecondIndex;

            RunAsync().Forget();
        }

        public virtual void Skip()
        {
            UniTaskEx.Cancel(this, 0);
            Invoke_OnBeforeInMissionStepChange();

            if (scenarioTable.HasNextFirstIndex == true)
            {
                recorder.ReadRecord(FirstIndex, SecondIndex, true, out int targetFirstIndex, out int targetSecondIndex);
                FirstIndex = targetFirstIndex;
                SecondIndex = targetSecondIndex;

                RunAsync().Forget();
            }
            else
            {
                Debug.LogFormat(LOG_FORMAT, "<color=red><b>시나리오 종료</b></color>");
                IsNextButtonClickable = false;
            }

            // recorder.FindNextRecordPoint(CurrentTaskRowList);
        }

#if DEBUG
        public static bool DEBUG_isShowGUI = true;

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.C) == true)
            {
                DEBUG_isShowGUI = !DEBUG_isShowGUI;
            }
        }

        protected virtual void OnGUI()
        {
            if (DEBUG_isShowGUI == true)
            {
                GUIStyle titleStyle = new GUIStyle();
                titleStyle.fontSize = 20;
                titleStyle.normal.textColor = Color.red;

                GUIStyle contentStyle = new GUIStyle();
                contentStyle.fontSize = 20;
                contentStyle.normal.textColor = Color.yellow;

                GUI.Label(new Rect(100f, 100f, 100f, 100f), "FirstIndex : \nSecondIndex : ", titleStyle);
                GUI.Label(new Rect(250f, 100f, 100f, 100f), FirstIndex + "\n" + SecondIndex, contentStyle);
            }
        }
#endif
    }
}