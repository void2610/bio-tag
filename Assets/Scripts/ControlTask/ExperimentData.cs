using Experiment;

namespace ControlTask
{
    /// <summary>
    /// 試行サマリーデータ（CSV形式で保存）
    /// </summary>
    public class TrialSummary : ICsvSerializable
    {
        public int TrialNumber;
        public string TargetState;
        public int StartTimeMS;  // 試行開始時刻（セッション開始からの経過時間）
        public float Score;
        public float SuccessRate;
        public float MeanGsr;
        public float SDGsr;
        public int ResponseTimeMS;

        // ICsvSerializable実装
        public string GetCsvHeader() => "trial_number,target_state,start_time_ms,score,success_rate,mean_gsr,sd_gsr,response_time_ms";
        public string ToCsvRow() => $"{TrialNumber},{TargetState},{StartTimeMS},{Score:F1},{SuccessRate:F2},{MeanGsr:F2},{SDGsr:F2},{ResponseTimeMS}";
    }

    /// <summary>
    /// 時系列データ（CSV形式で保存）
    /// </summary>
    public class TimeSeriesRecord : ICsvSerializable
    {
        public int TrialNumber;
        public int TimestampMS;
        public float GsrRaw;
        public float GsrDerivative;
        public float GsrThreshold;
        public string TargetValue;
        public string CurrentState;
        public int InstantaneousScore;
        public int CumulativeScore;

        // ICsvSerializable実装
        public string GetCsvHeader() => "trial_number,timestamp_ms,gsr_raw,gsr_derivative,gsr_threshold,target_value,current_state,instantaneous_score,cumulative_score";
        public string ToCsvRow() => $"{TrialNumber},{TimestampMS},{GsrRaw:F2},{GsrDerivative:F2},{GsrThreshold:F2},{TargetValue},{CurrentState},{InstantaneousScore},{CumulativeScore}";
    }
}
