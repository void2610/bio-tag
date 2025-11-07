using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VitalRouter;
using R3;
using BioTag.Biometric;

namespace ControlTask
{
    /// <summary>
    /// ControlTaskのPresenter層 - ビジネスロジックとフロー制御
    /// ModelとUIコンポーネントを直接仲介
    /// </summary>
    [Routes]
    public partial class ControlTaskPresenter
    {
        private readonly ControlTaskModel _model;
        private readonly IControlTaskService _service;
        private readonly GsrGraph _gsrGraph;
        private readonly TargetStateUI _targetStateUI;
        private readonly TimerUI _timerUI;
        private readonly ScoreUI _scoreUI;
        
        public void StartExperiment() => UpdateExperimentFlow().Forget();
        public void UpdateTime(float deltaTime) => _model.CurrentTime.Value += deltaTime;

        [Inject]
        public ControlTaskPresenter(
            ControlTaskModel model,
            IControlTaskService service,
            GsrGraph gsrGraph,
            TargetStateUI targetStateUI,
            TimerUI timerUI,
            ScoreUI scoreUI)
        {
            _model = model;
            _service = service;
            _gsrGraph = gsrGraph;
            _targetStateUI = targetStateUI;
            _timerUI = timerUI;
            _scoreUI = scoreUI;
        }

        /// <summary>
        /// Presenterを初期化 - Modelを購読してUIを更新
        /// </summary>
        public void Initialize()
        {
            // Modelを購読してUIコンポーネントを直接更新
            _model.TargetState
                .Subscribe(state => _targetStateUI.SetState(state))
                .AddTo(_targetStateUI);

            _model.CurrentTime
                .Subscribe(_ => _timerUI.SetRemainingTime(_model.CurrentPhaseRemainingTime))
                .AddTo(_timerUI);

            _model.Score
                .Subscribe(score => _scoreUI.SetScore(score))
                .AddTo(_scoreUI);
        }

        /// <summary>
        /// 実験フロー制御（UniTask）
        /// </summary>
        private async UniTaskVoid UpdateExperimentFlow()
        {
            // 1. キャリブレーションフェーズ
            _model.ChangePhase(ControlState.Calibration);
            Debug.Log("[ControlTaskPresenter] Calibration started");
            await UniTask.Delay((int)(_model.CalibrationDuration * 1000));

            // 2. 本測定（9試行）
            for (var i = 0; i < _model.TrialCount; i++)
            {
                var targetState = (i % 2 == 0) ? ControlState.Calmed : ControlState.Excited;

                // 目標提示
                _model.ChangePhase(ControlState.GoalPresentation);
                _model.CurrentTrialTargetState = targetState;
                Debug.Log($"[ControlTaskPresenter] Trial {i + 1}/{_model.TrialCount}: Goal = {targetState}");
                await UniTask.Delay((int)(_model.GoalPresentationDuration * 1000));

                // 準備期間
                _model.ChangePhase(ControlState.Preparation);
                await UniTask.Delay((int)(_model.PreparationDuration * 1000));

                // 測定期間
                _model.ChangePhase(targetState);
                _model.LastScore = _model.Score.Value;
                _service.StartTrial(targetState);
                await UniTask.Delay((int)(_model.MeasurementDuration * 1000));
                EndAndLogTrial(targetState);

                // フィードバック
                _model.ChangePhase(ControlState.Feedback);
                var trialScore = _model.Score.Value - _model.LastScore;
                Debug.Log($"[ControlTaskPresenter] Trial {i + 1} Score: {trialScore}");
                await UniTask.Delay((int)(_model.FeedbackDuration * 1000));

                // 休憩（設定されている場合）
                if (_model.RestDuration > 0)
                {
                    _model.ChangePhase(ControlState.Rest);
                    await UniTask.Delay((int)(_model.RestDuration * 1000));
                }
            }

            // 全試行完了
            Debug.Log($"[ControlTaskPresenter] All trials complete! Total Score: {_model.Score.Value}");
            _service.EndSession();
        }

        /// <summary>
        /// 試行終了とログ記録
        /// </summary>
        private void EndAndLogTrial(ControlState targetState)
        {
            var trialScore = _model.Score.Value - _model.LastScore;
            var maxPossibleScore = _model.MeasurementDuration * 60; // 60fps想定
            var successRate = maxPossibleScore > 0 ? trialScore / maxPossibleScore : 0f;

            _service.EndTrial(targetState, trialScore, successRate);
        }

        /// <summary>
        /// スコアリング更新（Tickableから呼ばれる）
        /// </summary>
        public void UpdateScoring()
        {
            var isCorrect = false;

            // 測定期間（Calmed or Excited）のみスコアをカウント
            if (_model.TargetState.Value == ControlState.Calmed || _model.TargetState.Value == ControlState.Excited)
            {
                // BiometricStateと目標状態を比較
                var targetBiometricState = _model.TargetState.Value == ControlState.Excited
                    ? BiometricState.Excited
                    : BiometricState.Calm;

                if (_model.CurrentBiometricState.Value == targetBiometricState)
                {
                    _model.Score.Value += 1;
                    isCorrect = true;
                }
            }

            // 時系列データの記録（測定期間のみ）
            if (_model.TargetState.Value is ControlState.Calmed or ControlState.Excited)
            {
                var currentState = _model.CurrentBiometricState.Value == BiometricState.Excited
                    ? ControlState.Excited
                    : ControlState.Calmed;
                var instantaneousScore = isCorrect ? 100 : 0;

                _service.RecordTimeSeriesData(
                    _gsrGraph.CurrentGsrRaw,
                    _model.TargetState.Value,
                    currentState,
                    instantaneousScore,
                    _model.Score.Value
                );
            }
        }


        /// <summary>
        /// 生体状態変化コマンドハンドラ
        /// </summary>
        [Route]
        private void On(BiometricStateChangedCommand cmd)
        {
            _model.CurrentBiometricState.Value = cmd.NewState;
        }
    }
}
