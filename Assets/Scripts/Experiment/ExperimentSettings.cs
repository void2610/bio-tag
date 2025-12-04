using System;
using System.IO;
using System.Text.RegularExpressions;
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
                    testType = GetAutoTestType().ToString()
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
            return $"{participantId}_{GetAutoTestType()}";
        }

        /// <summary>
        /// 既存データから次のゲーム試行番号を取得
        /// </summary>
        /// <param name="gameMode">ゲームモード（PlayerVsNPC, PlayerVsPlayer等）</param>
        /// <returns>次の試行番号</returns>
        public int GetNextTrialNumber(string gameMode)
        {
            var baseDirectory = Path.Combine(Application.persistentDataPath, "ExperimentData");

            if (!Directory.Exists(baseDirectory))
                return 1;

            // パターン: {participantId}_{gameMode}_Trial{N}_timestamp
            var pattern = $@"^{Regex.Escape(participantId)}_{Regex.Escape(gameMode)}_Trial(\d+)_";
            var regex = new Regex(pattern);

            int maxTrialNumber = 0;

            foreach (var dir in Directory.GetDirectories(baseDirectory))
            {
                var dirName = Path.GetFileName(dir);
                var match = regex.Match(dirName);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int trialNum))
                {
                    maxTrialNumber = Math.Max(maxTrialNumber, trialNum);
                }
            }

            return maxTrialNumber + 1;
        }

        /// <summary>
        /// 既存データからテスト種類（Pre/Post）を自動判定
        /// </summary>
        /// <returns>判定されたテスト種類</returns>
        public TestType GetAutoTestType()
        {
            var baseDirectory = Path.Combine(Application.persistentDataPath, "ExperimentData");

            if (!Directory.Exists(baseDirectory))
                return TestType.Pre;

            // パターン: {participantId}_Pre_timestamp
            var pattern = $@"^{Regex.Escape(participantId)}_Pre_";
            var regex = new Regex(pattern);

            foreach (var dir in Directory.GetDirectories(baseDirectory))
            {
                var dirName = Path.GetFileName(dir);
                if (regex.IsMatch(dirName))
                    return TestType.Post; // Preが存在する → Post
            }

            return TestType.Pre; // Preが存在しない → Pre
        }
    }
}
