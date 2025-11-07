using UnityEngine;
using UnityEngine.VFX;
using VContainer;
using BioTag.Camera;

namespace ControlTask
{
    /// <summary>
    /// GSRグラフのパーティクルエフェクトとカメラシェイクを制御するView
    /// GraphParticle.csをVContainer対応にリファクタリング
    /// </summary>
    public class GraphParticleView : MonoBehaviour
    {
        [SerializeField] private GsrGraph graph;

        private VisualEffect _graphParticle;
        private bool _isPlaying = false;

        private ControlTaskModel _model;
        private CameraEffectService _cameraEffect;

        /// <summary>
        /// VContainer経由でModelとCameraEffectServiceを注入
        /// </summary>
        [Inject]
        public void Construct(ControlTaskModel model, CameraEffectService cameraEffect)
        {
            _model = model;
            _cameraEffect = cameraEffect;
        }

        private void Awake()
        {
            _graphParticle = this.GetComponent<VisualEffect>();
            _graphParticle.Stop();
        }

        private void Start()
        {
            graph.SetLineColor(Color.white);
        }

        private void Update()
        {
            // 目標状態と現在の生体状態が不一致の場合パーティクル再生
            var target = _model.TargetState.Value == ControlState.Excited;
            if (graph.IsExcited != target && !_isPlaying)
            {
                _isPlaying = true;
                _graphParticle.Play();
                graph.SetLineColor(Color.red);
                _cameraEffect.StartShake(0.25f);
            }
            else if (!graph.IsExcited != target && _isPlaying)
            {
                _isPlaying = false;
                _graphParticle.Stop();
                graph.SetLineColor(Color.white);
                _cameraEffect.StopShake();
            }

            // パーティクル位置をグラフの最終データに追従
            this.transform.position = graph.GetLastData();
        }
    }
}
