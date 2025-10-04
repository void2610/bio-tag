using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// 実験セッションの管理クラス
    /// - セッションディレクトリの作成・管理
    /// - JSON保存機能
    /// - 複数CSVファイルの一括管理
    /// </summary>
    public class ExperimentSession : IDisposable
    {
        private readonly string _sessionDirectory;
        private readonly List<IDisposable> _csvWriters = new();
        private bool _isDisposed = false;

        /// <summary>
        /// セッションディレクトリのパス
        /// </summary>
        public string SessionDirectory => _sessionDirectory;

        /// <summary>
        /// 実験セッションを初期化
        /// </summary>
        /// <param name="sessionName">セッション名（タイムスタンプが自動追加される）</param>
        /// <param name="baseDirectory">ベースディレクトリ（省略時はApplication.persistentDataPath/ExperimentData）</param>
        public ExperimentSession(string sessionName, string baseDirectory = null)
        {
            // ベースディレクトリのデフォルト設定
            if (string.IsNullOrEmpty(baseDirectory))
            {
                baseDirectory = Path.Combine(Application.persistentDataPath, "ExperimentData");
            }

            // タイムスタンプ付きディレクトリ名を生成
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dirName = $"{sessionName}_{timestamp}";
            _sessionDirectory = Path.Combine(baseDirectory, dirName);

            // ディレクトリを作成
            Directory.CreateDirectory(_sessionDirectory);

            Debug.Log($"[ExperimentSession] Session started: {_sessionDirectory}");
        }

        /// <summary>
        /// 指定したファイル名の完全パスを取得
        /// </summary>
        public string GetFilePath(string filename)
        {
            return Path.Combine(_sessionDirectory, filename);
        }

        /// <summary>
        /// オブジェクトをJSON形式で保存
        /// </summary>
        /// <param name="filename">ファイル名（例: "session.json"）</param>
        /// <param name="data">保存するオブジェクト</param>
        /// <param name="prettyPrint">整形して保存するか（デフォルト: true）</param>
        public void SaveJson<T>(string filename, T data, bool prettyPrint = true)
        {
            var json = JsonUtility.ToJson(data, prettyPrint);
            var filePath = GetFilePath(filename);
            File.WriteAllText(filePath, json);
            Debug.Log($"[ExperimentSession] JSON saved: {filename}");
        }

        /// <summary>
        /// CSVライターを作成し、セッション管理下に追加
        /// </summary>
        /// <param name="filename">CSVファイル名（例: "trial_summary.csv"）</param>
        /// <param name="headerProvider">ヘッダー生成用のサンプルオブジェクト</param>
        /// <param name="bufferFlushThreshold">バッファフラッシュ閾値</param>
        /// <returns>作成されたCsvWriter</returns>
        public CsvWriter<T> CreateCsvWriter<T>(string filename, T headerProvider = default, int bufferFlushThreshold = 100)
            where T : ICsvSerializable
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ExperimentSession));

            var filePath = GetFilePath(filename);
            var writer = new CsvWriter<T>(filePath, headerProvider, bufferFlushThreshold);
            _csvWriters.Add(writer);

            Debug.Log($"[ExperimentSession] CSV writer created: {filename}");
            return writer;
        }

        /// <summary>
        /// セッション終了とすべてのCSVファイルをクローズ
        /// </summary>
        public void EndSession()
        {
            if (_isDisposed)
                return;

            // すべてのCSVライターをDispose
            foreach (var writer in _csvWriters)
            {
                writer?.Dispose();
            }
            _csvWriters.Clear();

            Debug.Log($"[ExperimentSession] Session ended: {_sessionDirectory}");
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            EndSession();
            _isDisposed = true;
        }
    }
}
