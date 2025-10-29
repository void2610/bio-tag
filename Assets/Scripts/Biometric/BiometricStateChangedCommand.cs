using VitalRouter;

namespace BioTag.Biometric
{
    /// <summary>
    /// 生体状態 (GSR由来)
    /// </summary>
    public enum BiometricState
    {
        /// <summary>冷静状態</summary>
        Calm,
        /// <summary>興奮状態</summary>
        Excited
    }

    /// <summary>
    /// 生体状態変化コマンド
    /// GsrGraphがIsExcitedの状態変化を検知した際に発行される
    /// </summary>
    public readonly struct BiometricStateChangedCommand : ICommand
    {
        /// <summary>
        /// 新しい生体状態
        /// </summary>
        public readonly BiometricState NewState;

        /// <summary>
        /// 以前の生体状態
        /// </summary>
        public readonly BiometricState PreviousState;

        /// <summary>
        /// GSR強度 (現在値)
        /// </summary>
        public readonly float Intensity;

        public BiometricStateChangedCommand(BiometricState newState, BiometricState previousState, float intensity)
        {
            NewState = newState;
            PreviousState = previousState;
            Intensity = intensity;
        }
    }
}
