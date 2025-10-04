using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ControlTask
{
    public enum ControlState
    {
        Calibration,      // キャリブレーション
        GoalPresentation, // 目標提示
        Preparation,      // 準備期間
        Excited,          // 測定（興奮）
        Calmed,           // 測定（冷静）
        Feedback,         // フィードバック
        Rest              // 休憩
    }
    public class ControlTaskManager : MonoBehaviour
    {
        public static ControlTaskManager Instance;

        [SerializeField] private GsrGraph gsrGraph;

        [Header("Calibration Settings")]
        [SerializeField] private float calibrationDuration = 30f;

        [Header("Trial Settings")]
        [SerializeField] private int trialCount = 9;
        [SerializeField] private float goalPresentationDuration = 3f;
        [SerializeField] private float preparationDuration = 2f;
        [SerializeField] private float measurementDuration = 20f;
        [SerializeField] private float feedbackDuration = 5f;
        [SerializeField] private float restDuration = 0f;

        [Header("Experiment Settings")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private string participantId = "P001";
        [SerializeField] private ExperimentGroup experimentGroup = ExperimentGroup.BfHuman;
        [SerializeField] private TestType testType = TestType.Pre;
        [SerializeField] private float roomTemperature = 23.5f;
        [SerializeField] private float roomHumidity = 45.0f;

        [Header("Baseline GSR Settings")]
        [SerializeField] private float baselineGsr = 2.45f;
        [SerializeField] private float minGsr = 1.82f;
        [SerializeField] private float maxGsr = 4.31f;

        private ControlTaskDataLogger _dataLogger;

        public readonly ReactiveProperty<ControlState> TargetState = new();
        public readonly ReactiveProperty<int> Score = new(0);
        public readonly ReactiveProperty<float> CurrentTime = new(0);

        // 総実験時間 = キャリブレーション + (試行時間 × 試行数)
        public float TotalDuration => calibrationDuration +
            (goalPresentationDuration + preparationDuration + measurementDuration + feedbackDuration + restDuration) * trialCount;

        // 現在のフェーズの開始時刻
        private float _phaseStartTime = 0f;

        // 現在のフェーズの持続時間
        public float CurrentPhaseDuration
        {
            get
            {
                return TargetState.Value switch
                {
                    ControlState.Calibration => calibrationDuration,
                    ControlState.GoalPresentation => goalPresentationDuration,
                    ControlState.Preparation => preparationDuration,
                    ControlState.Calmed => measurementDuration,
                    ControlState.Excited => measurementDuration,
                    ControlState.Feedback => feedbackDuration,
                    ControlState.Rest => restDuration,
                    _ => 0f
                };
            }
        }

        // 現在のフェーズの残り時間
        public float CurrentPhaseRemainingTime
        {
            get
            {
                var elapsed = CurrentTime.Value - _phaseStartTime;
                var remaining = CurrentPhaseDuration - elapsed;
                return Mathf.Max(0f, remaining);
            }
        }

        // 実験フロー: キャリブレーション → 9試行（目標提示 → 準備 → 測定 → フィードバック → 休憩）
        private async UniTaskVoid UpdateTarget()
        {
            // 1. キャリブレーションフェーズ
            _phaseStartTime = CurrentTime.Value;
            TargetState.Value = ControlState.Calibration;
            Debug.Log("[ControlTask] Calibration started");
            await UniTask.Delay((int)(calibrationDuration * 1000));

            // 2. 本測定（9試行）
            for (var i = 0; i < trialCount; i++)
            {
                var targetState = (i % 2 == 0) ? ControlState.Calmed : ControlState.Excited; // 交互に切り替え

                // 目標提示
                _phaseStartTime = CurrentTime.Value;
                TargetState.Value = ControlState.GoalPresentation;
                _currentTrialTargetState = targetState; // 目標状態を記録
                Debug.Log($"[ControlTask] Trial {i + 1}/{trialCount}: Goal = {targetState}");
                await UniTask.Delay((int)(goalPresentationDuration * 1000));

                // 準備期間
                _phaseStartTime = CurrentTime.Value;
                TargetState.Value = ControlState.Preparation;
                await UniTask.Delay((int)(preparationDuration * 1000));

                // 測定期間
                _phaseStartTime = CurrentTime.Value;
                _lastScore = Score.Value;
                TargetState.Value = targetState;
                if (enableLogging) _dataLogger.StartTrial(targetState);
                await UniTask.Delay((int)(measurementDuration * 1000));
                if (enableLogging) EndAndLogTrial(targetState);

                // フィードバック
                _phaseStartTime = CurrentTime.Value;
                TargetState.Value = ControlState.Feedback;
                _trialScore = Score.Value - _lastScore; // 試行スコアを記録
                Debug.Log($"[ControlTask] Trial {i + 1} Score: {_trialScore}");
                await UniTask.Delay((int)(feedbackDuration * 1000));

                // 休憩（設定されている場合）
                if (restDuration > 0)
                {
                    _phaseStartTime = CurrentTime.Value;
                    TargetState.Value = ControlState.Rest;
                    await UniTask.Delay((int)(restDuration * 1000));
                }
            }

            // 全試行完了
            Debug.Log($"[ControlTask] All trials complete! Total Score: {Score.Value}");
            if (enableLogging) _dataLogger.EndSession();
        }

        private ControlState _currentTrialTargetState; // 現在の試行の目標状態
        private int _trialScore; // 試行ごとのスコア

        /// <summary>
        /// 試行終了とログ記録
        /// </summary>
        private void EndAndLogTrial(ControlState targetState)
        {
            var trialScore = Score.Value - _lastScore;
            var maxPossibleScore = measurementDuration * 60; // 60fps想定
            var successRate = maxPossibleScore > 0 ? trialScore / maxPossibleScore : 0f;

            _dataLogger.EndTrial(targetState, trialScore, successRate);
        }
        private int _lastScore = 0; // 試行ごとのスコア追跡用

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            // データロガーの初期化
            if (enableLogging)
            {
                _dataLogger = gameObject.AddComponent<ControlTaskDataLogger>();
                InitializeSession();
            }

            UpdateTarget().Forget();
        }

        /// <summary>
        /// セッション初期化
        /// </summary>
        private void InitializeSession()
        {
            var sessionInfo = new SessionInfo
            {
                participantInfo = new ParticipantInfo
                {
                    participantID = participantId,
                    group = experimentGroup.ToString(),
                    testType = testType.ToString()
                },
                calibration = new CalibrationData
                {
                    baselineGsr = baselineGsr,
                    minGsr = minGsr,
                    maxGsr = maxGsr,
                    calibrationDurationMS = (int)(calibrationDuration * 1000)
                },
                roomTemperature = roomTemperature,
                roomHumidity = roomHumidity
            };

            _dataLogger.StartSession(sessionInfo);
        }

        private void Update()
        {
            var isCorrect = false;

            // 測定期間（Calmed or Excited）のみスコアをカウント
            if (TargetState.Value == ControlState.Calmed || TargetState.Value == ControlState.Excited)
            {
                if (gsrGraph.IsExcited == (TargetState.Value == ControlState.Excited))
                {
                    Score.Value += 1;
                    isCorrect = true;
                }
            }

            // 経過時間を記録
            CurrentTime.Value += Time.deltaTime;

            // 時系列データの記録（測定期間のみ）
            if (enableLogging && _dataLogger &&
                TargetState.Value is ControlState.Calmed or ControlState.Excited)
            {
                var currentState = gsrGraph.IsExcited ? ControlState.Excited : ControlState.Calmed;
                var instantaneousScore = isCorrect ? 100 : 0;

                _dataLogger.RecordTimeSeriesData(
                    gsrGraph.CurrentGsrRaw,
                    TargetState.Value,
                    currentState,
                    instantaneousScore,
                    Score.Value
                );
            }
        }
    }
}
