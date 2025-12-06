using UnityEngine;
using VContainer;
using VContainer.Unity;
using BioTag.Camera;
using Experiment;

namespace ControlTask
{
    /// <summary>
    /// ControlTaskシーンのエントリーポイント
    /// 初期化とメインループを管理
    /// </summary>
    public class ControlTaskEntryPoint : IStartable, ITickable
    {
        private readonly ControlTaskModel _model;
        private readonly ControlTaskPresenter _presenter;
        private readonly IControlTaskService _service;
        private readonly CameraEffectService _cameraEffect;
        private readonly TcpServer _tcpServer; // Optional: GsrMock使用時はnull
        private readonly ExperimentSettings _experimentSettings;

        // 実験開始フラグ
        private bool _experimentStarted = false;

        [Inject]
        public ControlTaskEntryPoint(
            ControlTaskModel model,
            ControlTaskPresenter presenter,
            IControlTaskService service,
            CameraEffectService cameraEffect,
            IObjectResolver resolver)
        {
            _model = model;
            _presenter = presenter;
            _service = service;
            _cameraEffect = cameraEffect;

            // TcpServerをOptionalに解決（GsrMock使用時はnull）
            resolver.TryResolve<TcpServer>(out _tcpServer);

            // ExperimentSettingsをOptionalに解決
            resolver.TryResolve<ExperimentSettings>(out _experimentSettings);
        }

        public void Start()
        {
            // Presenterを初期化（Model購読→View更新）
            _presenter.Initialize();

            // カメラエフェクトサービスを初期化
            _cameraEffect.Initialize(Camera.main);

            // TcpServer使用時は接続待機、GsrMock時は即座に開始
            if (_tcpServer != null)
            {
                Debug.Log("[ControlTaskEntryPoint] TcpServer接続待機中...");
            }
            else
            {
                Debug.Log("[ControlTaskEntryPoint] GsrMock使用 - 即座に実験開始");
                StartExperimentFlow();
            }
        }

        /// <summary>
        /// 実験フローを開始
        /// </summary>
        private void StartExperimentFlow()
        {
            if (_experimentStarted) return;
            _experimentStarted = true;

            // ExperimentSettingsからセッション情報を生成
            if (_experimentSettings != null)
            {
                var calibrationDurationMS = (int)(_model.CalibrationDuration * 1000);
                var sessionInfo = _experimentSettings.CreateSessionInfo(calibrationDurationMS);

                // セッション開始
                _service.StartSession(sessionInfo);
            }
            else
            {
                Debug.LogWarning("[ControlTaskEntryPoint] ExperimentSettings not found, skipping session logging");
            }

            // 実験フロー開始
            _presenter.StartExperiment();

            Debug.Log("[ControlTaskEntryPoint] ControlTask started");
        }

        public void Tick()
        {
            // TcpServer使用時は接続確認後に実験開始
            if (!_experimentStarted && _tcpServer != null)
            {
                if (_tcpServer.IsConnected)
                {
                    Debug.Log("[ControlTaskEntryPoint] TcpServer接続完了 - 実験開始");
                    StartExperimentFlow();
                }
                return; // 接続待機中はUpdateを実行しない
            }

            // 実験開始後の通常更新
            if (_experimentStarted)
            {
                _presenter.UpdateTime(Time.deltaTime);
                _presenter.UpdateScoring(Time.deltaTime);
            }
        }
    }
}
