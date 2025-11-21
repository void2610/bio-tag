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
        private readonly List<float> _gsrRawHistory = new();  // 生の値の履歴（微分計算用）
        private readonly List<float> _gsrHistory = new();     // 微分値の履歴（グラフ表示用）
        private readonly int _historyLength;

        // フィルタ・閾値設定
        private readonly int _derivativeWindowSize;
        private float _threshold;
        private readonly float _thresholdMagnification;
        private readonly float _checkLength;

        // 現在値
        public float CurrentGsrRaw { get; private set; }
        public float CurrentGsrFiltered { get; private set; }
        public float CurrentGsrDerivative { get; private set; } = 0f;
        public float CurrentThreshold => _threshold;
        public float Baseline { get; private set; }
        public bool IsExcited { get; private set; }

        // 状態変化検知用
        private bool _previousIsExcited;

        public List<float> GetDataHistory() => _gsrHistory.Select(value => value - Baseline).ToList();
        public void AdjustThreshold(float delta) => _threshold += delta;
        public void SetBaseline(float baseline) => Baseline = baseline;
        public void AdjustBaseline(float delta) => Baseline += delta;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="historyLength">履歴保持数（デフォルト500）</param>
        /// <param name="filterWindowSize">移動平均ウィンドウサイズ（デフォルト10）</param>
        /// <param name="derivativeWindowSize">微分計算のウィンドウサイズ（デフォルト10）</param>
        /// <param name="baseline">基準値（デフォルト512.0）</param>
        /// <param name="threshold">興奮判定閾値（デフォルト5.0）</param>
        /// <param name="thresholdMagnification">閾値倍率（デフォルト1.5）</param>
        /// <param name="checkLength">チェック範囲（デフォルト0.1 = 10%）</param>
        public GsrProcessorService(
            int historyLength = 500,
            int derivativeWindowSize = 10,
            float baseline = 512f,
            float threshold = 5f,
            float thresholdMagnification = 1.5f,
            float checkLength = 0.1f)
        {
            _historyLength = historyLength;
            _derivativeWindowSize = derivativeWindowSize;
            Baseline = baseline;
            _threshold = threshold;
            _thresholdMagnification = thresholdMagnification;
            _checkLength = checkLength;

            // 履歴を初期化
            for (var i = 0; i < _historyLength; i++)
            {
                _gsrRawHistory.Add(0f);
                _gsrHistory.Add(0f);
            }
        }

        /// <summary>
        /// 自動キャリブレーション
        /// 現在の履歴データの平均値をベースラインに設定
        /// </summary>
        public void AutoCalibrate()
        {
            if (_gsrRawHistory.Count == 0) return;
            SetBaseline(GetMean());
        }

        /// <summary>
        /// GSRデータの平均値を計算
        /// </summary>
        public float GetRawMean()
        {
            if (_gsrRawHistory.Count == 0) return 0f;
            return _gsrRawHistory.Average();
        }

        public float GetMean()
        {
            if (_gsrRawHistory.Count == 0) return 0f;
            return _gsrHistory.Average();
        }

        /// <summary>
        /// GSRデータの標準偏差を計算
        /// </summary>
        public float GetStandardDeviation()
        {
            if (_gsrRawHistory.Count == 0) return 0f;
            var mean = GetMean();
            var sumOfSquares = _gsrRawHistory.Sum(v => (v - mean) * (v - mean));
            return Mathf.Sqrt(sumOfSquares / _gsrRawHistory.Count);
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

            // 生の値の履歴にデータを追加（古いデータを削除）
            for (int i = 0; i < _historyLength - 1; i++)
            {
                _gsrRawHistory[i] = _gsrRawHistory[i + 1];
            }
            _gsrRawHistory[_historyLength - 1] = rawValue;

            // 微分値を計算（windowSize分前の値との差分）
            // 履歴が十分に溜まっている場合のみ計算
            var nonZeroCount = _gsrRawHistory.Count(v => v != 0f);
            if (nonZeroCount >= _derivativeWindowSize)
            {
                var pastValue = _gsrRawHistory[_historyLength - _derivativeWindowSize];
                CurrentGsrDerivative = rawValue - pastValue;
            }
            else
            {
                CurrentGsrDerivative = 0f;
            }

            // 微分値の履歴にデータを追加（グラフ表示用）
            for (int i = 0; i < _historyLength - 1; i++)
            {
                _gsrHistory[i] = _gsrHistory[i + 1];
            }
            _gsrHistory[_historyLength - 1] = CurrentGsrDerivative;

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
        /// 興奮状態を判定
        /// ベースラインからの差分が閾値を超えた場合にExcitedと判定
        /// </summary>
        private bool CheckExcited()
        {
            // 最新の値をベースラインからの差分でチェック
            var latestDiff = Mathf.Abs(_gsrRawHistory[^1] - Baseline);
            if (latestDiff > _threshold)
                return true;

            // 過去の値をチェック（閾値の倍率を大きく超えると判定）
            var significantThreshold = _threshold * _thresholdMagnification;
            var checkCount = Mathf.FloorToInt(_checkLength * _historyLength);
            for (var i = _gsrRawHistory.Count - 1; i >= 0 && _gsrRawHistory.Count - 1 - i < checkCount; i--)
            {
                var diff = Mathf.Abs(_gsrRawHistory[i] - Baseline);
                if (diff > significantThreshold) return true;
            }

            return false;
        }
    }
}
