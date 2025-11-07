using UnityEngine;
using DG.Tweening;
using VContainer;

namespace BioTag.Camera
{
    /// <summary>
    /// カメラエフェクトサービス - カメラシェイク機能を提供
    /// CameraMove.csのSingletonをVContainer対応に変換
    /// </summary>
    public class CameraEffectService
    {
        private UnityEngine.Camera _mainCamera;
        private Transform _cameraTransform;
        private Vector3 _initialPosition;
        private Tween _cameraShakeTween;

        /// <summary>
        /// カメラの初期化
        /// </summary>
        public void Initialize(UnityEngine.Camera camera)
        {
            _mainCamera = camera;
            _cameraTransform = camera.transform;
            _initialPosition = _cameraTransform.position;
        }

        /// <summary>
        /// 連続シェイクを開始（ループ）
        /// </summary>
        public void StartShake(float strength)
        {
            if (_cameraTransform == null) return;

            var s = Mathf.Min(7.5f, strength);
            _cameraShakeTween?.Kill();
            _cameraShakeTween = _cameraTransform.DOShakePosition(0.1f, s, 10, 0, false).SetLoops(-1);
        }

        /// <summary>
        /// 連続シェイクを停止
        /// </summary>
        public void StopShake()
        {
            if (_cameraTransform == null) return;

            _cameraShakeTween?.Kill();
            _cameraTransform.position = _initialPosition;
        }

        /// <summary>
        /// ワンショットシェイク
        /// </summary>
        public void ShakeCamera(float duration, float strength)
        {
            if (_cameraTransform == null) return;

            var s = Mathf.Min(7.5f, strength);
            _cameraTransform.DOShakePosition(duration, s, 10, 0, false).OnComplete(() =>
            {
                _cameraTransform.position = _initialPosition;
            });
        }
    }
}
