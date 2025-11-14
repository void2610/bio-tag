using UnityEngine;
using UnityEngine.VFX;
using VContainer;
using BioTag.Camera;
using BioTag.Biometric;

namespace ControlTask
{
    /// <summary>
    /// GSRグラフのパーティクルエフェクトとカメラシェイクを制御するView
    /// GraphParticle.csをVContainer対応にリファクタリング
    /// </summary>
    public class GraphParticleView : MonoBehaviour
    {
        [SerializeField] private GsrGraphView graphView;

        private VisualEffect _graphParticle;
        private bool _isPlaying = false;

        private ControlTaskModel _model;
        private CameraEffectService _cameraEffect;
        private GsrProcessorService _gsrProcessor;

        /// <summary>
        /// VContainer経由でModelとCameraEffectServiceを注入
        /// </summary>
        [Inject]
        public void Construct(ControlTaskModel model, CameraEffectService cameraEffect, GsrProcessorService gsrProcessor)
        {
            _model = model;
            _cameraEffect = cameraEffect;
            _gsrProcessor = gsrProcessor;
        }

        private void Awake()
        {
            _graphParticle = this.GetComponent<VisualEffect>();
            _graphParticle.Stop();
        }

        private void Start()
        {
            graphView.SetLineColor(Color.white);
        }

        private void Update()
        {
            // 目標状態と現在の生体状態が不一致の場合パーティクル再生
            var target = _model.TargetState.Value == ControlState.Excited;
            if (_gsrProcessor.IsExcited != target && !_isPlaying)
            {
                _isPlaying = true;
                _graphParticle.Play();
                graphView.SetLineColor(Color.red);
                _cameraEffect.StartShake(0.25f);
            }
            else if (!_gsrProcessor.IsExcited != target && _isPlaying)
            {
                _isPlaying = false;
                _graphParticle.Stop();
                graphView.SetLineColor(Color.white);
                _cameraEffect.StopShake();
            }

            // パーティクル位置をグラフの最終データに追従
            this.transform.position = graphView.GetLastData();
        }
    }
}
