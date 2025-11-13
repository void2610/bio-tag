namespace ControlTask
{
    /// <summary>
    /// ControlTask実験管理サービスのインターフェース
    /// </summary>
    public interface IControlTaskService
    {
        /// <summary>
        /// ロギングが有効かどうか
        /// </summary>
        bool EnableLogging { get; set; }

        /// <summary>
        /// 実験セッションを開始
        /// </summary>
        void StartSession(SessionInfo sessionInfo);

        /// <summary>
        /// 試行を開始
        /// </summary>
        void StartTrial(ControlState targetState);

        /// <summary>
        /// 時系列データを記録
        /// </summary>
        void RecordTimeSeriesData(float gsrRaw, float gsrFiltered, float gsrDerivative, float gsrThreshold,
                                 ControlState targetState, ControlState currentState,
                                 int instantaneousScore, int cumulativeScore);

        /// <summary>
        /// 試行を終了してサマリーを記録
        /// </summary>
        void EndTrial(ControlState targetState, int score, float successRate);

        /// <summary>
        /// セッションを終了
        /// </summary>
        void EndSession();
    }
}
