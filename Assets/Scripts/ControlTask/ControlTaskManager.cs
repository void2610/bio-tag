using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ControlTask
{
    public enum ControlState
    {
        Excited,
        Calmed,
        Rest
    }
    public class ControlTaskManager : MonoBehaviour
    {
        public static ControlTaskManager Instance;

        [SerializeField] private GsrGraph gsrGraph;

        [Header("Task Settings")]
        [SerializeField] private float calmedDuration = 15f;
        [SerializeField] private float excitedDuration = 15f;
        [SerializeField] private float restDuration = 5f;
        [SerializeField] private int repeatCount = 2;

        [Header("Experiment Settings")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private string participantId = "P001";
        [SerializeField] private ExperimentGroup experimentGroup = ExperimentGroup.BfHuman;
        [SerializeField] private TestType testType = TestType.Pre;
        [SerializeField] private float roomTemperature = 23.5f;
        [SerializeField] private float roomHumidity = 45.0f;

        [Header("Calibration Settings")]
        [SerializeField] private float baselineGsr = 2.45f;
        [SerializeField] private float minGsr = 1.82f;
        [SerializeField] private float maxGsr = 4.31f;
        [SerializeField] private int calibrationDurationMs = 30000;

        private ControlTaskDataLogger _dataLogger;

        public readonly ReactiveProperty<ControlState> TargetState = new();
        public readonly ReactiveProperty<int> Score = new(0);
        public readonly ReactiveProperty<float> CurrentTime = new(0);
        
        public float TotalDuration => (calmedDuration + excitedDuration + 2 * restDuration) * repeatCount;

        // 固定パターン: Calmed → Rest → Excited → Rest を繰り返す
        private async UniTaskVoid UpdateTarget()
        {
            for (var i = 0; i < repeatCount; i++)
            {
                // Calmed試行
                _lastScore = Score.Value;
                TargetState.Value = ControlState.Calmed;
                if (enableLogging) _dataLogger.StartTrial(ControlState.Calmed);
                await UniTask.Delay((int)(calmedDuration * 1000));
                if (enableLogging) EndAndLogTrial(ControlState.Calmed);

                // 休憩
                TargetState.Value = ControlState.Rest;
                await UniTask.Delay((int)(restDuration * 1000));

                // Excited試行
                _lastScore = Score.Value;
                TargetState.Value = ControlState.Excited;
                if (enableLogging) _dataLogger.StartTrial(ControlState.Excited);
                await UniTask.Delay((int)(excitedDuration * 1000));
                if (enableLogging) EndAndLogTrial(ControlState.Excited);

                // 休憩
                TargetState.Value = ControlState.Rest;
                await UniTask.Delay((int)(restDuration * 1000));
            }

            // 全パターン完了後、タスク終了
            Debug.Log($"Task Complete! Score: {Score.Value}");
            if (enableLogging) _dataLogger.EndSession();
        }

        /// <summary>
        /// 試行終了とログ記録
        /// </summary>
        private void EndAndLogTrial(ControlState targetState)
        {
            var trialScore = Score.Value - _lastScore;
            var maxPossibleScore = (targetState == ControlState.Calmed ? calmedDuration : excitedDuration) * 60; // 60fps想定
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
                    calibrationDurationMS = calibrationDurationMs
                },
                roomTemperature = roomTemperature,
                roomHumidity = roomHumidity
            };

            _dataLogger.StartSession(sessionInfo);
        }

        private void Update()
        {
            var isCorrect = false;

            // Rest状態ではスコアをカウントしない
            if (TargetState.Value != ControlState.Rest)
            {
                if (gsrGraph.IsExcited == (TargetState.Value == ControlState.Excited))
                {
                    Score.Value += 1;
                    isCorrect = true;
                }
            }

            // 経過時間を記録
            CurrentTime.Value += Time.deltaTime;

            // 時系列データの記録（Rest以外）
            if (enableLogging && _dataLogger != null && TargetState.Value != ControlState.Rest)
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
