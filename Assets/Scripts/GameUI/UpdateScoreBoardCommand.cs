using System.Collections.Generic;
using VitalRouter;

namespace BioTag.GameUI
{
    /// <summary>
    /// スコアボード更新コマンド
    /// プレイヤー名とスコアのリストを渡す
    /// </summary>
    public readonly struct UpdateScoreBoardCommand : ICommand
    {
        public readonly IReadOnlyList<string> PlayerNames;
        public readonly IReadOnlyList<float> PlayerScores;

        public UpdateScoreBoardCommand(IReadOnlyList<string> playerNames, IReadOnlyList<float> playerScores)
        {
            PlayerNames = playerNames;
            PlayerScores = playerScores;
        }
    }
}
