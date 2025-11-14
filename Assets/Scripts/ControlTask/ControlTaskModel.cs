using R3;
using UnityEngine;
using BioTag.Biometric;

namespace ControlTask
{
    /// <summary>
    /// ControlTask実験の状態
    /// </summary>
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

    /// <summary>
    /// ControlTaskのModel層 - データと状態を管理
    /// </summary>
    public class ControlTaskModel
    {
        // 状態のReactiveProperty
        public readonly ReactiveProperty<ControlState> TargetState = new();
        public readonly ReactiveProperty<int> Score = new(0);
        public readonly ReactiveProperty<float> CurrentTime = new(0);
        public readonly ReactiveProperty<BiometricState> CurrentBiometricState = new(BiometricState.Calm);

        // 実験設定
        public float CalibrationDuration { get; }
        public int TrialCount { get; }
        public float GoalPresentationDuration { get; }
        public float PreparationDuration { get; }
        public float MeasurementDuration { get; }
        public float FeedbackDuration { get; }
        public float RestDuration { get; }

        // フェーズ管理
        public float PhaseStartTime { get; set; } = 0f;
        public ControlState CurrentTrialTargetState { get; set; }
        public int LastScore { get; set; } = 0;

        /// <summary>
        /// コンストラクタ - ExperimentConfigから設定を初期化
        /// </summary>
        public ControlTaskModel(ExperimentConfig config)
        {
            CalibrationDuration = config.calibrationDuration;
            TrialCount = config.trialCount;
            GoalPresentationDuration = config.goalPresentationDuration;
            PreparationDuration = config.preparationDuration;
            MeasurementDuration = config.measurementDuration;
            FeedbackDuration = config.feedbackDuration;
            RestDuration = config.restDuration;
        }

        // 総実験時間
        public float TotalDuration => CalibrationDuration +
            (GoalPresentationDuration + PreparationDuration + MeasurementDuration + FeedbackDuration + RestDuration) * TrialCount;

        // 現在のフェーズの持続時間
        public float CurrentPhaseDuration
        {
            get
            {
                return TargetState.Value switch
                {
                    ControlState.Calibration => CalibrationDuration,
                    ControlState.GoalPresentation => GoalPresentationDuration,
                    ControlState.Preparation => PreparationDuration,
                    ControlState.Calmed => MeasurementDuration,
                    ControlState.Excited => MeasurementDuration,
                    ControlState.Feedback => FeedbackDuration,
                    ControlState.Rest => RestDuration,
                    _ => 0f
                };
            }
        }

        // 現在のフェーズの残り時間
        public float CurrentPhaseRemainingTime
        {
            get
            {
                var elapsed = CurrentTime.Value - PhaseStartTime;
                var remaining = CurrentPhaseDuration - elapsed;
                return Mathf.Max(0f, remaining);
            }
        }

        /// <summary>
        /// フェーズを変更（開始時刻を記録）
        /// </summary>
        public void ChangePhase(ControlState newState)
        {
            PhaseStartTime = CurrentTime.Value;
            TargetState.Value = newState;
        }
    }
}
