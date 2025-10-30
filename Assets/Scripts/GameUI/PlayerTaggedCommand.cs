using VitalRouter;

namespace BioTag.GameUI
{
    /// <summary>
    /// プレイヤータグ付けコマンド
    /// プレイヤーが別のプレイヤーにタッチして「鬼」を交代したときに発行
    /// </summary>
    public readonly struct PlayerTaggedCommand : ICommand
    {
        public readonly int TaggedPlayerIndex;

        public PlayerTaggedCommand(int taggedPlayerIndex)
        {
            TaggedPlayerIndex = taggedPlayerIndex;
        }
    }
}
