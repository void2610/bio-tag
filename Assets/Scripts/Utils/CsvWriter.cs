using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Experiment
{
    /// <summary>
    /// CSV形式でシリアライズ可能なデータ構造のインターフェース
    /// </summary>
    public interface ICsvSerializable
    {
        /// <summary>
        /// CSVヘッダー行を取得
        /// </summary>
        string GetCsvHeader();

        /// <summary>
        /// CSV行データに変換
        /// </summary>
        string ToCsvRow();
    }

    /// <summary>
    /// 汎用CSVライター（バッファリング機能付き）
    /// </summary>
    /// <typeparam name="T">CSV形式でシリアライズ可能な型</typeparam>
    public class CsvWriter<T> : IDisposable where T : ICsvSerializable
    {
        private readonly string _filePath;
        private readonly int _bufferFlushThreshold;
        private StreamWriter _writer;
        private List<T> _buffer = new();
        private bool _isDisposed = false;

        /// <summary>
        /// CSVライターを初期化
        /// </summary>
        /// <param name="filePath">出力先CSVファイルパス</param>
        /// <param name="headerProvider">ヘッダー生成用のサンプルオブジェクト（省略可）</param>
        /// <param name="bufferFlushThreshold">バッファフラッシュ閾値（デフォルト: 100）</param>
        public CsvWriter(string filePath, T headerProvider = default, int bufferFlushThreshold = 100)
        {
            _filePath = filePath;
            _bufferFlushThreshold = bufferFlushThreshold;

            // ディレクトリの作成（存在しない場合）
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // CSVファイルを作成してヘッダーを書き込み
            _writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // ヘッダープロバイダーが提供されている場合、またはデフォルト値が存在する場合はヘッダー書き込み
            if (headerProvider != null)
            {
                _writer.WriteLine(headerProvider.GetCsvHeader());
                _writer.Flush();
            }
            else if (default(T) != null)
            {
                _writer.WriteLine(default(T).GetCsvHeader());
                _writer.Flush();
            }
        }

        /// <summary>
        /// レコードを書き込み（バッファリング）
        /// </summary>
        /// <param name="record">書き込むレコード</param>
        public void WriteRecord(T record)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(CsvWriter<T>));

            _buffer.Add(record);

            // バッファが閾値に達したらフラッシュ
            if (_buffer.Count >= _bufferFlushThreshold)
            {
                Flush();
            }
        }

        /// <summary>
        /// バッファをディスクに書き込み
        /// </summary>
        public void Flush()
        {
            if (_isDisposed || _buffer.Count == 0)
                return;

            foreach (var record in _buffer)
            {
                _writer.WriteLine(record.ToCsvRow());
            }
            _writer.Flush();
            _buffer.Clear();
        }

        /// <summary>
        /// ファイルを閉じて、バッファをフラッシュ
        /// </summary>
        public void Close()
        {
            if (_isDisposed)
                return;

            Flush();
            _writer?.Close();
            _writer = null;
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            Close();
            _isDisposed = true;
        }
    }
}
