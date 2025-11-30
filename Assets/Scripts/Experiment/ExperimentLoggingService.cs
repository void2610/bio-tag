using System;
using UnityEngine;

namespace Experiment
{
    /// <summary>
    /// 実験ロギングサービス
    /// セッション管理とメタデータ保存を行う共通サービス
    /// </summary>
    public class ExperimentLoggingService : IDisposable
    {
        private ExperimentSession _session;
        private readonly ExperimentSettings _settings;
        private bool _isDisposed;

        /// <summary>
        /// ロギングが有効かどうか
        /// </summary>
        public bool EnableLogging => _settings != null && _settings.enableLogging;

        /// <summary>
        /// セッションディレクトリのパス
        /// </summary>
        public string SessionDirectory => _session?.SessionDirectory;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settings">実験設定</param>
        public ExperimentLoggingService(ExperimentSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// セッションを開始
        /// </summary>
        /// <param name="sessionNameSuffix">セッション名のサフィックス（例: "ControlTask", "TagGame"）</param>
        /// <param name="calibrationDurationMS">キャリブレーション時間（ミリ秒）</param>
        public void StartSession(string sessionNameSuffix = null, int? calibrationDurationMS = null)
        {
            if (!EnableLogging) return;

            // セッション名を生成
            var sessionName = _settings.GetSessionName();
            if (!string.IsNullOrEmpty(sessionNameSuffix))
            {
                sessionName = $"{sessionName}_{sessionNameSuffix}";
            }

            // セッションを作成
            _session = new ExperimentSession(sessionName);

            // セッション情報をJSONで保存
            var sessionInfo = _settings.CreateSessionInfo(calibrationDurationMS);
            _session.SaveJson("session.json", sessionInfo);

            Debug.Log($"[ExperimentLoggingService] Session started: {_session.SessionDirectory}");
        }

        /// <summary>
        /// CSVライターを作成
        /// </summary>
        public CsvWriter<T> CreateCsvWriter<T>(string filename, T headerProvider = default, int bufferFlushThreshold = 100)
            where T : ICsvSerializable
        {
            if (!EnableLogging || _session == null)
            {
                Debug.LogWarning("[ExperimentLoggingService] Cannot create CSV writer: logging disabled or session not started");
                return null;
            }

            return _session.CreateCsvWriter(filename, headerProvider, bufferFlushThreshold);
        }

        /// <summary>
        /// 追加のJSONデータを保存
        /// </summary>
        public void SaveJson<T>(string filename, T data, bool prettyPrint = true)
        {
            if (!EnableLogging || _session == null)
            {
                Debug.LogWarning("[ExperimentLoggingService] Cannot save JSON: logging disabled or session not started");
                return;
            }

            _session.SaveJson(filename, data, prettyPrint);
        }

        /// <summary>
        /// セッションを終了
        /// </summary>
        public void EndSession()
        {
            if (_session == null) return;

            _session.Dispose();
            Debug.Log($"[ExperimentLoggingService] Session ended: {_session.SessionDirectory}");
            _session = null;
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            EndSession();
            _isDisposed = true;
        }
    }
}
