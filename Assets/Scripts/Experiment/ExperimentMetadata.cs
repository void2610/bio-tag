using System;

namespace Experiment
{
    /// <summary>
    /// 実験グループの定義
    /// </summary>
    public enum ExperimentGroup
    {
        BfHuman,      // バイオフィードバックあり_人間対戦
        // ReSharper disable once InconsistentNaming
        BFNpc,       // バイオフィードバックあり_NPC対戦
        NoBfHuman,    // バイオフィードバックなし_人間対戦
        NoBfNpc       // バイオフィードバックなし_NPC対戦
    }

    /// <summary>
    /// テスト種類の定義
    /// </summary>
    public enum TestType
    {
        Pre,   // 事前テスト
        Post   // 事後テスト
    }

    /// <summary>
    /// セッション情報（JSON形式で保存）
    /// </summary>
    [Serializable]
    public class SessionInfo
    {
        public ParticipantInfo participantInfo;
        public CalibrationData calibration;
        public string datetime;
        public float roomTemperature;
        public float roomHumidity;
    }

    /// <summary>
    /// 参加者情報
    /// </summary>
    [Serializable]
    public class ParticipantInfo
    {
        public string participantID;
        public string group;
        public string testType;
    }

    /// <summary>
    /// キャリブレーションデータ
    /// </summary>
    [Serializable]
    public class CalibrationData
    {
        public float baselineGsr;
        public float thresholdGsr;
        public int calibrationDurationMS;
    }
}
