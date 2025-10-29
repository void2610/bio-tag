using UnityEngine;
using VitalRouter;

namespace BioTag.Audio
{
    /// <summary>
    /// 足音種別
    /// </summary>
    public enum FootstepType
    {
        /// <summary>歩行中の足音</summary>
        Step,
        /// <summary>着地音</summary>
        Landing
    }

    /// <summary>
    /// 足音再生コマンド (歩行音・着地音共通)
    /// プレイヤーのアニメーションイベントから送信される
    /// </summary>
    public readonly struct PlayFootstepCommand : ICommand
    {
        /// <summary>
        /// 音を再生する3D空間座標
        /// </summary>
        public readonly Vector3 Position;

        /// <summary>
        /// 足音の種別
        /// </summary>
        public readonly FootstepType Type;

        public PlayFootstepCommand(Vector3 position, FootstepType type)
        {
            Position = position;
            Type = type;
        }
    }
}
