using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using DG.Tweening;
using VitalRouter;

namespace BioTag.Biometric
{
    /// <summary>
    /// 生体状態変化に応答するサービス
    /// VFX、Vignette、プレイヤー速度などを制御
    /// </summary>
    [Routes]
    public partial class BiometricService
    {
        private readonly List<VisualEffect> _vfxList = new();
        private readonly List<Tween> _tweenList = new();
        private readonly IPlayerSpawnService _playerSpawn;
        private Volume _volume;

        public BiometricService(IPlayerSpawnService playerSpawn)
        {
            _playerSpawn = playerSpawn;
        }

        /// <summary>
        /// VFXをサービスに登録
        /// </summary>
        public void AddVFX(VisualEffect vfx)
        {
            _vfxList.Add(vfx);
        }

        /// <summary>
        /// Volumeを設定 (Vignette制御用)
        /// </summary>
        public void SetVolume(Volume volume)
        {
            _volume = volume;
        }

        /// <summary>
        /// 生体状態変化コマンドハンドラ
        /// </summary>
        [Route]
        private void On(BiometricStateChangedCommand cmd)
        {
            switch (cmd.NewState)
            {
                case BiometricState.Excited:
                    ChangeToExcited();
                    break;
                case BiometricState.Calm:
                    ChangeToCalm();
                    break;
            }
        }

        /// <summary>
        /// 興奮状態への遷移処理
        /// </summary>
        private void ChangeToExcited()
        {
            Debug.Log("BiometricService: Excited");

            // 既存のTweenをキャンセル
            foreach (var t in _tweenList)
            {
                t.Kill();
            }
            _tweenList.Clear();

            // VFXアルファを0にフェード (赤色表示)
            SetAlpha(0.0f, 1f);

            // Vignette強度を上げる
            if (_volume != null && _volume.profile.TryGet(out Vignette vignette))
            {
                var tw = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0.5f, 1f);
                _tweenList.Add(tw);
            }

            // プレイヤー速度を下げる (興奮時は遅く)
            var mainPlayer = GetMainPlayer();
            if (mainPlayer) mainPlayer.SetWalkSpeed(3f);
        }

        /// <summary>
        /// 冷静状態への遷移処理
        /// </summary>
        private void ChangeToCalm()
        {
            Debug.Log("BiometricService: Calm");

            // 既存のTweenをキャンセル
            foreach (var t in _tweenList)
            {
                t.Kill();
            }
            _tweenList.Clear();

            // VFXアルファを1にフェード (白色表示)
            SetAlpha(1.0f, 3f);

            // Vignette強度を下げる
            if (_volume != null && _volume.profile.TryGet(out Vignette vignette))
            {
                var tw = DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0f, 1f);
                _tweenList.Add(tw);
            }

            // プレイヤー速度を上げる (冷静時は速く)
            var mainPlayer = GetMainPlayer();
            if (mainPlayer) mainPlayer.SetWalkSpeed(6.5f);
        }

        /// <summary>
        /// VFXのアルファ値を設定
        /// </summary>
        private void SetAlpha(float alpha, float duration)
        {
            foreach (var vfx in _vfxList)
            {
                var t = DOTween.To(() => vfx.GetFloat("alpha"), x => vfx.SetFloat("alpha", x), alpha, duration);
                _tweenList.Add(t);
            }
        }

        /// <summary>
        /// メインプレイヤーを取得
        /// </summary>
        private PlayerBase GetMainPlayer()
        {
            if (_playerSpawn?.SpawnedPlayers is { Count: > 0 })
            {
                var playerObj = _playerSpawn.SpawnedPlayers[0];
                if (playerObj)
                {
                    return playerObj.GetComponent<PlayerBase>();
                }
            }
            return null;
        }
    }
}
