using VitalRouter;

namespace BioTag.GameUI
{
    /// <summary>
    /// タイマー更新コマンド
    /// ゲーム中の経過時間が変化した際にEntryPointから発行される
    /// </summary>
    public readonly struct UpdateTimerCommand : ICommand
    {
        /// <summary>
        /// ゲーム経過時間 (秒)
        /// </summary>
        public readonly float ElapsedTime;

        public UpdateTimerCommand(float elapsedTime)
        {
            ElapsedTime = elapsedTime;
        }
    }
}
