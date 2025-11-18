using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VitalRouter;

namespace BioTag.Biometric
{
    /// <summary>
    /// GSRデータ処理サービス
    /// フィルタリング、微分計算、興奮状態判定を担当
    /// UI表示とは独立したデータ処理層
    /// </summary>
    [Routes]
    public partial class GsrProcessorService
    {
        // データ保持
        private readonly List<float> _gsrHistory = new();
        private readonly int _historyLength;

        // フィルタ・閾値設定
        private readonly int _filterWindowSize;
        private float _threshold;
        private readonly float _thresholdMagnification;
        private readonly float _checkLength;

        // 現在値
        public float CurrentGsrRaw { get; private set; }
        public float CurrentGsrFiltered { get; private set; }
        public float CurrentThreshold => _threshold;
        public bool IsExcited { get; private set; }

        // 状態変化検知用
        private bool _previousIsExcited;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="historyLength">履歴保持数（デフォルト500）</param>
        /// <param name="filterWindowSize">移動平均ウィンドウサイズ（デフォルト10）</param>
        /// <param name="threshold">興奮判定閾値（デフォルト5.0）</param>
        /// <param name="thresholdMagnification">閾値倍率（デフォルト1.5）</param>
        /// <param name="checkLength">チェック範囲（デフォルト0.1 = 10%）</param>
        public GsrProcessorService(
            int historyLength = 500,
            int filterWindowSize = 10,
            float threshold = 5f,
            float thresholdMagnification = 1.5f,
            float checkLength = 0.1f)
        {
            _historyLength = historyLength;
            _filterWindowSize = filterWindowSize;
            _threshold = threshold;
            _thresholdMagnification = thresholdMagnification;
            _checkLength = checkLength;

            // 履歴を初期化
            for (int i = 0; i < _historyLength; i++)
            {
                _gsrHistory.Add(0f);
            }
        }

        /// <summary>
        /// GSRデータ受信コマンドハンドラ
        /// </summary>
        [Route]
        private void On(GsrDataReceivedCommand cmd)
        {
            AddData(cmd.RawValue);
        }

        /// <summary>
        /// GSRデータを追加して処理
        /// </summary>
        private void AddData(float rawValue)
        {
            rawValue = Mathf.Clamp(rawValue, 0f, 1024f);

            CurrentGsrRaw = rawValue;

            // 履歴にデータを追加（古いデータを削除）
            for (int i = 0; i < _historyLength - 1; i++)
            {
                _gsrHistory[i] = _gsrHistory[i + 1];
            }
            _gsrHistory[_historyLength - 1] = rawValue;

            // フィルタ済み値を計算（移動平均）
            CurrentGsrFiltered = CalculateFilteredValue();

            // 興奮状態を判定
            var newIsExcited = CheckExcited();

            // 状態変化を検知してCommandを発行
            if (newIsExcited != _previousIsExcited)
            {
                IsExcited = newIsExcited;
                var newState = IsExcited ? BiometricState.Excited : BiometricState.Calm;
                var previousState = _previousIsExcited ? BiometricState.Excited : BiometricState.Calm;

                Router.Default.PublishAsync(new BiometricStateChangedCommand(newState, previousState, rawValue));

                _previousIsExcited = IsExcited;
            }
            else
            {
                IsExcited = newIsExcited;
            }
        }

        /// <summary>
        /// フィルタ済みGSR値を計算（移動平均）
        /// </summary>
        private float CalculateFilteredValue()
        {
            if (_gsrHistory.Count == 0) return 0f;

            var windowSize = Mathf.Min(_filterWindowSize, _gsrHistory.Count);
            var sum = 0f;
            for (var i = _gsrHistory.Count - windowSize; i < _gsrHistory.Count; i++)
            {
                sum += _gsrHistory[i];
            }
            return sum / windowSize;
        }

        /// <summary>
        /// 興奮状態を判定
        /// </summary>
        private bool CheckExcited()
        {
            // 最新の値をチェック
            if (Mathf.Abs(_gsrHistory[^1]) > _threshold)
                return true;

            // 過去の値をチェック（閾値の倍率を大きく超えると判定）
            var significantThreshold = _threshold * _thresholdMagnification;
            var checkCount = Mathf.FloorToInt(_checkLength * _historyLength);
            for (var i = _gsrHistory.Count - 1; i >= 0 && _gsrHistory.Count - 1 - i < checkCount; i--)
            {
                if (Mathf.Abs(_gsrHistory[i]) > significantThreshold) return true;
            }

            return false;
        }

        /// <summary>
        /// GSRデータの平均値を計算
        /// </summary>
        public float GetMean()
        {
            if (_gsrHistory.Count == 0) return 0f;
            return _gsrHistory.Average();
        }

        /// <summary>
        /// GSRデータの標準偏差を計算
        /// </summary>
        public float GetStandardDeviation()
        {
            if (_gsrHistory.Count == 0) return 0f;
            var mean = GetMean();
            var sumOfSquares = _gsrHistory.Sum(v => (v - mean) * (v - mean));
            return Mathf.Sqrt(sumOfSquares / _gsrHistory.Count);
        }

        /// <summary>
        /// データ履歴を取得（UI描画用）
        /// </summary>
        public List<float> GetDataHistory()
        {
            return new List<float>(_gsrHistory);
        }

        /// <summary>
        /// 閾値を手動調整（デバッグ用）
        /// </summary>
        public void AdjustThreshold(float delta)
        {
            _threshold += delta;
            Debug.Log($"[GsrProcessorService] Threshold adjusted to {_threshold}");
        }
    }
}
