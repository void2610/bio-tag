using UnityEngine;
using VContainer;
using VContainer.Unity;
using BioTag.Camera;

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

        // 実験設定（LifetimeScopeから注入）
        private readonly string _participantId;
        private readonly ExperimentGroup _experimentGroup;
        private readonly TestType _testType;
        private readonly float _roomTemperature;
        private readonly float _roomHumidity;
        private readonly float _baselineGsr;
        private readonly float _minGsr;
        private readonly float _maxGsr;

        // 実験開始フラグ
        private bool _experimentStarted = false;

        [Inject]
        public ControlTaskEntryPoint(
            ControlTaskModel model,
            ControlTaskPresenter presenter,
            IControlTaskService service,
            CameraEffectService cameraEffect,
            IObjectResolver resolver,
            string participantId,
            ExperimentGroup experimentGroup,
            TestType testType,
            float roomTemperature,
            float roomHumidity,
            float baselineGsr,
            float minGsr,
            float maxGsr)
        {
            _model = model;
            _presenter = presenter;
            _service = service;
            _cameraEffect = cameraEffect;

            // TcpServerをOptionalに解決（GsrMock使用時はnull）
            resolver.TryResolve<TcpServer>(out _tcpServer);

            _participantId = participantId;
            _experimentGroup = experimentGroup;
            _testType = testType;
            _roomTemperature = roomTemperature;
            _roomHumidity = roomHumidity;
            _baselineGsr = baselineGsr;
            _minGsr = minGsr;
            _maxGsr = maxGsr;
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

            // セッション情報を生成
            var sessionInfo = new SessionInfo
            {
                participantInfo = new ParticipantInfo
                {
                    participantID = _participantId,
                    group = _experimentGroup.ToString(),
                    testType = _testType.ToString()
                },
                calibration = new CalibrationData
                {
                    baselineGsr = _baselineGsr,
                    minGsr = _minGsr,
                    maxGsr = _maxGsr,
                    calibrationDurationMS = (int)(_model.CalibrationDuration * 1000)
                },
                roomTemperature = _roomTemperature,
                roomHumidity = _roomHumidity
            };

            // セッション開始
            _service.StartSession(sessionInfo);

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
                _presenter.UpdateScoring();
            }
        }
    }
}
