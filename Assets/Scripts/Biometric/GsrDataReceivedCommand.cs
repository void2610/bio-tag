using VitalRouter;

namespace BioTag.Biometric
{
    /// <summary>
    /// GSRデータ受信コマンド
    /// TCPサーバー、モック、テストなど各種データソースから発行される
    /// GsrGraphが受信してグラフに追加
    /// </summary>
    public readonly struct GsrDataReceivedCommand : ICommand
    {
        /// <summary>
        /// GSR生データ値 (0-1024)
        /// </summary>
        public readonly float RawValue;

        public GsrDataReceivedCommand(float rawValue)
        {
            RawValue = rawValue;
        }
    }
}
