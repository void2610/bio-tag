using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Experiment;

namespace ControlTask
{
    /// <summary>
    /// ControlTask実験管理サービス - ロギング機能を提供
    /// ControlTaskDataLoggerの機能を統合
    /// </summary>
    public class ControlTaskService : IControlTaskService, IDisposable
    {
        private ExperimentSession _session;
        private CsvWriter<TrialSummary> _trialSummaryWriter;
        private CsvWriter<TimeSeriesRecord> _timeSeriesWriter;

        private SessionInfo _sessionInfo;
        private int _currentTrialNumber;
        private float _trialStartTime;
        private List<float> _trialGsrData = new();

        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// 実験セッションを開始
        /// </summary>
        public void StartSession(SessionInfo sessionInfo)
        {
            if (!EnableLogging) return;

            _sessionInfo = sessionInfo;
            _sessionInfo.datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            var sessionName = $"{sessionInfo.participantInfo.participantID}_{sessionInfo.participantInfo.testType}";
            _session = new ExperimentSession(sessionName);

            // セッション情報をJSONで保存
            _session.SaveJson("session.json", _sessionInfo);

            // CSVファイルの初期化
            _trialSummaryWriter = _session.CreateCsvWriter("trial_summary.csv", new TrialSummary());
            _timeSeriesWriter = _session.CreateCsvWriter("timeseries.csv", new TimeSeriesRecord());

            Debug.Log($"[ControlTaskService] Session started: {_session.SessionDirectory}");
        }

        /// <summary>
        /// 試行を開始
        /// </summary>
        public void StartTrial(ControlState targetState)
        {
            if (!EnableLogging) return;

            _currentTrialNumber++;
            _trialStartTime = Time.time;
            _trialGsrData.Clear();

            Debug.Log($"[ControlTaskService] Trial {_currentTrialNumber} started: Target={targetState}");
        }

        /// <summary>
        /// 時系列データを記録
        /// </summary>
        public void RecordTimeSeriesData(float gsrRaw, ControlState targetState, ControlState currentState,
                                        int instantaneousScore, int cumulativeScore)
        {
            if (!EnableLogging) return;

            var timestamp = (int)((Time.time - _trialStartTime) * 1000); // ミリ秒

            var record = new TimeSeriesRecord
            {
                ParticipantID = _sessionInfo.participantInfo.participantID,
                TestType = _sessionInfo.participantInfo.testType,
                TrialNumber = _currentTrialNumber,
                TimestampMS = timestamp,
                GsrRaw = gsrRaw,
                TargetValue = targetState.ToString(),
                CurrentState = currentState.ToString(),
                InstantaneousScore = instantaneousScore,
                CumulativeScore = cumulativeScore
            };

            _timeSeriesWriter.WriteRecord(record);
            _trialGsrData.Add(gsrRaw);
        }

        /// <summary>
        /// 試行を終了してサマリーを記録
        /// </summary>
        public void EndTrial(ControlState targetState, int score, float successRate)
        {
            if (!EnableLogging || _trialGsrData.Count == 0) return;

            var duration = (Time.time - _trialStartTime) * 1000; // ミリ秒
            var meanGsr = _trialGsrData.Average();
            var sdGsr = CalculateStandardDeviation(_trialGsrData);

            var summary = new TrialSummary
            {
                ParticipantID = _sessionInfo.participantInfo.participantID,
                TestType = _sessionInfo.participantInfo.testType,
                TrialNumber = _currentTrialNumber,
                TargetState = targetState.ToString(),
                DurationMS = (int)duration,
                Score = score,
                SuccessRate = successRate,
                MeanGsr = meanGsr,
                SDGsr = sdGsr,
                ResponseTimeMS = 0 // TODO: 必要に応じて実装
            };

            _trialSummaryWriter.WriteRecord(summary);
            _trialSummaryWriter.Flush();

            Debug.Log($"[ControlTaskService] Trial {_currentTrialNumber} ended: Score={score}, SuccessRate={successRate:F2}");
        }

        /// <summary>
        /// セッションを終了
        /// </summary>
        public void EndSession()
        {
            if (!EnableLogging) return;

            _timeSeriesWriter?.Flush();
            _session?.Dispose();
            Debug.Log($"[ControlTaskService] Session ended. Data saved to: {_session?.SessionDirectory}");
        }

        /// <summary>
        /// 標準偏差の計算
        /// </summary>
        private float CalculateStandardDeviation(List<float> values)
        {
            if (values.Count == 0) return 0f;

            var mean = values.Average();
            var sumOfSquares = values.Sum(v => (v - mean) * (v - mean));
            return Mathf.Sqrt(sumOfSquares / values.Count);
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            EndSession();
        }
    }
}
