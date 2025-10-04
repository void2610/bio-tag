using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace ControlTask
{
    /// <summary>
    /// ControlTask実験データのロギングを管理するクラス
    /// </summary>
    public class ControlTaskDataLogger : IDisposable
    {
        private ExperimentSession _session;
        private CsvWriter<TrialSummary> _trialSummaryWriter;
        private CsvWriter<TimeSeriesRecord> _timeSeriesWriter;

        private SessionInfo _sessionInfo;

        private int _currentTrialNumber;
        private float _trialStartTime;
        private List<float> _trialGsrData = new();

        /// <summary>
        /// セッション開始（ディレクトリとファイルの作成）
        /// </summary>
        public void StartSession(SessionInfo sessionInfo)
        {
            _sessionInfo = sessionInfo;
            _sessionInfo.datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            var sessionName = $"{sessionInfo.participantInfo.participantID}_{sessionInfo.participantInfo.testType}";
            // セッションの生成
            _session = new ExperimentSession(sessionName);

            // セッション情報をJSONで保存
            _session.SaveJson("session.json", _sessionInfo);

            // CSVファイルの初期化
            InitializeCsvFiles();
        }

        /// <summary>
        /// CSVファイルの初期化（ヘッダー書き込み）
        /// </summary>
        private void InitializeCsvFiles()
        {
            // 試行サマリーCSV
            _trialSummaryWriter = _session.CreateCsvWriter("trial_summary.csv", new TrialSummary());
            // 時系列データCSV
            _timeSeriesWriter = _session.CreateCsvWriter("timeseries.csv", new TimeSeriesRecord());

            Debug.Log("[ExperimentData] CSV files initialized");
        }

        /// <summary>
        /// 試行開始
        /// </summary>
        public void StartTrial(ControlState targetState)
        {
            _currentTrialNumber++;
            _trialStartTime = Time.time;
            _trialGsrData.Clear();

            Debug.Log($"[ExperimentData] Trial {_currentTrialNumber} started: Target={targetState}");
        }

        /// <summary>
        /// 試行終了とサマリー記録
        /// </summary>
        public void EndTrial(ControlState targetState, int score, float successRate)
        {
            if (_trialGsrData.Count == 0) return;

            var duration = (Time.time - _trialStartTime) * 1000; // ミリ秒に変換
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
                ResponseTimeMS = 0 // TODO: 実装が必要な場合
            };

            _trialSummaryWriter.WriteRecord(summary);
            _trialSummaryWriter.Flush();

            Debug.Log($"[ExperimentData] Trial {_currentTrialNumber} ended: Score={score}, SuccessRate={successRate:F2}");
        }

        /// <summary>
        /// 時系列データの記録（高頻度呼び出し用）
        /// </summary>
        public void RecordTimeSeriesData(float gsrRaw, ControlState targetState, ControlState currentState,
                                         int instantaneousScore, int cumulativeScore)
        {
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
        /// セッション終了とクリーンアップ
        /// </summary>
        public void EndSession()
        {
            _session?.Dispose();
            Debug.Log($"[ExperimentData] Session ended. Data saved to: {_session?.SessionDirectory}");
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
