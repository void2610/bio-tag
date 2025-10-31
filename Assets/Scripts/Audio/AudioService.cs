using UnityEngine;
using VitalRouter;

namespace BioTag.Audio
{
    /// <summary>
    /// ゲーム内の音響処理を一元管理するサービス
    /// VitalRouterを通じてCommandを受信し、AudioSource.PlayClipAtPointで再生
    /// </summary>
    [Routes]
    public partial class AudioService
    {
        private readonly AudioConfig _config;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="config">音響設定</param>
        public AudioService(AudioConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// 足音再生コマンドハンドラ (歩行音・着地音共通)
        /// </summary>
        [Route]
        private void On(PlayFootstepCommand cmd)
        {
            switch (cmd.Type)
            {
                case FootstepType.Step:
                    PlayStepSound(cmd.Position);
                    break;
                case FootstepType.Landing:
                    PlayLandingSound(cmd.Position);
                    break;
            }
        }

        /// <summary>
        /// 歩行中の足音を再生
        /// </summary>
        private void PlayStepSound(Vector3 position)
        {
            var clipIndex = Random.Range(0, _config.footstepClips.Length);
            var clip = _config.footstepClips[clipIndex];
            AudioSource.PlayClipAtPoint(clip, position, _config.footstepVolume);
        }

        /// <summary>
        /// 着地音を再生
        /// </summary>
        private void PlayLandingSound(Vector3 position)
        {
            AudioSource.PlayClipAtPoint(_config.landingClip, position, _config.landingVolume);
        }
    }
}
