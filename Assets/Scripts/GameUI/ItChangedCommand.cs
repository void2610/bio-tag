using UnityEngine;
using VitalRouter;

namespace BioTag.GameUI
{
    /// <summary>
    /// "It"プレイヤー変更コマンド
    /// ゲーム中に鬼プレイヤーが交代した際にGameManagerServiceから発行される
    /// ItMarkerの追跡対象を更新するために使用
    /// </summary>
    public readonly struct ItChangedCommand : ICommand
    {
        public readonly int NewItIndex;
        public readonly string ItName;
        public readonly Transform TargetTransform;

        public ItChangedCommand(int newItIndex, string itName, Transform targetTransform)
        {
            NewItIndex = newItIndex;
            ItName = itName;
            TargetTransform = targetTransform;
        }
    }
}
