using System;
using UnityEngine;

namespace Experiment
{
    /// <summary>
    /// 実験設定用ScriptableObject
    /// 全シーンで共有可能な実験メタデータ設定
    /// </summary>
    [CreateAssetMenu(fileName = "ExperimentSettings", menuName = "Experiment/Settings")]
    public class ExperimentSettings : ScriptableObject
    {
        [Header("ログ設定")]
        [Tooltip("実験データのログ記録を有効にするか")]
        public bool enableLogging = true;

        [Header("参加者情報")]
        [Tooltip("参加者ID")]
        public string participantId = "P001";

        [Tooltip("実験グループ")]
        public ExperimentGroup experimentGroup = ExperimentGroup.BfHuman;

        [Tooltip("テスト種類（事前/事後）")]
        public TestType testType = TestType.Pre;

        [Header("環境情報")]
        [Tooltip("室温（℃）")]
        public float roomTemperature = 23.5f;

        [Tooltip("湿度（%）")]
        public float roomHumidity = 45.0f;

        [Header("キャリブレーション設定")]
        [Tooltip("ベースラインGSR値（処理済み値）")]
        public float baselineGsr = 2.45f;

        [Tooltip("閾値GSR値（処理済み値）")]
        public float thresholdGsr = 3.0f;

        /// <summary>
        /// セッション情報を生成
        /// </summary>
        /// <param name="calibrationDurationMS">キャリブレーション時間（ミリ秒）。nullの場合は0</param>
        /// <returns>セッション情報</returns>
        public SessionInfo CreateSessionInfo(int? calibrationDurationMS = null)
        {
            return new SessionInfo
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
                    thresholdGsr = thresholdGsr,
                    calibrationDurationMS = calibrationDurationMS ?? 0
                },
                datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                roomTemperature = roomTemperature,
                roomHumidity = roomHumidity
            };
        }

        /// <summary>
        /// セッション名を生成（ディレクトリ名等に使用）
        /// </summary>
        public string GetSessionName()
        {
            return $"{participantId}_{testType}";
        }
    }
}
