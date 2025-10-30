using VitalRouter;

namespace BioTag.GameUI
{
    /// <summary>
    /// "It"プレイヤー更新コマンド
    /// 鬼交代時にGameManagerServiceから発行される
    /// </summary>
    public readonly struct UpdateItPlayerCommand : ICommand
    {
        /// <summary>
        /// "It"プレイヤーのインデックス
        /// </summary>
        public readonly int ItIndex;

        /// <summary>
        /// "It"プレイヤーの名前
        /// </summary>
        public readonly string ItName;

        public UpdateItPlayerCommand(int itIndex, string itName)
        {
            ItIndex = itIndex;
            ItName = itName;
        }
    }
}
