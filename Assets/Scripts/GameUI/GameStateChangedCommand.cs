using VitalRouter;

namespace BioTag.GameUI
{
    /// <summary>
    /// ゲーム状態変更コマンド
    /// 0=Waiting, 1=Playing, 2=GameOver
    /// </summary>
    public readonly struct GameStateChangedCommand : ICommand
    {
        public readonly int NewState;
        public readonly int? PreviousState;

        public GameStateChangedCommand(int newState, int? previousState = null)
        {
            NewState = newState;
            PreviousState = previousState;
        }
    }
}
