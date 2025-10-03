using System;
using System.Collections.Generic;

namespace ControlTask
{
    // 実験グループの定義
    public enum ExperimentGroup
    {
        BfHuman,      // バイオフィードバックあり_人間対戦
        BF_NPC,        // バイオフィードバックあり_NPC対戦
        NoBfHuman,    // バイオフィードバックなし_人間対戦
        NoBfNpc       // バイオフィードバックなし_NPC対戦
    }

    // テスト種類の定義
    public enum TestType
    {
        Pre,   // 事前テスト
        Post   // 事後テスト
    }

    // セッション情報（JSON形式で保存）
    [Serializable]
    public class SessionInfo
    {
        public ParticipantInfo participantInfo;
        public CalibrationData calibration;
        public string datetime;
        public float roomTemperature;
        public float roomHumidity;
    }

    [Serializable]
    public class ParticipantInfo
    {
        public string participantID;
        public string group;
        public string testType;
    }

    [Serializable]
    public class CalibrationData
    {
        public float baselineGsr;
        public float minGsr;
        public float maxGsr;
        public int calibrationDurationMS;
    }

    // 試行サマリーデータ（CSV形式で保存）
    public class TrialSummary
    {
        public string ParticipantID;
        public string TestType;
        public int TrialNumber;
        public string TargetState;
        public int DurationMS;
        public float Score;
        public float SuccessRate;
        public float MeanGsr;
        public float SDGsr;
        public int ResponseTimeMS;

        // CSVヘッダー
        public static string CsvHeader =>
            "participant_id,test_type,trial_number,target_state,duration_ms,score,success_rate,mean_gsr,sd_gsr,response_time_ms";

        // CSV行に変換
        public string ToCsvRow() =>
            $"{ParticipantID},{TestType},{TrialNumber},{TargetState},{DurationMS},{Score:F1},{SuccessRate:F2},{MeanGsr:F2},{SDGsr:F2},{ResponseTimeMS}";
    }

    // 時系列データ（CSV形式で保存）
    public class TimeSeriesRecord
    {
        public string ParticipantID;
        public string TestType;
        public int TrialNumber;
        public int TimestampMS;
        public float GsrRaw;
        public string TargetValue;
        public string CurrentState;
        public int InstantaneousScore;
        public int CumulativeScore;

        // CSVヘッダー
        public static string CsvHeader =>
            "participant_id,test_type,trial_number,timestamp_ms,gsr_raw,target_value,current_state,instantaneous_score,cumulative_score";

        // CSV行に変換
        public string ToCsvRow() =>
            $"{ParticipantID},{TestType},{TrialNumber},{TimestampMS},{GsrRaw:F2},{TargetValue},{CurrentState},{InstantaneousScore},{CumulativeScore}";
    }
}
