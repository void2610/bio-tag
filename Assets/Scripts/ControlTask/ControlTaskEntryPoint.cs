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

        // 実験設定（LifetimeScopeから注入）
        private readonly string _participantId;
        private readonly ExperimentGroup _experimentGroup;
        private readonly TestType _testType;
        private readonly float _roomTemperature;
        private readonly float _roomHumidity;
        private readonly float _baselineGsr;
        private readonly float _minGsr;
        private readonly float _maxGsr;

        [Inject]
        public ControlTaskEntryPoint(
            ControlTaskModel model,
            ControlTaskPresenter presenter,
            IControlTaskService service,
            CameraEffectService cameraEffect,
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
            _presenter.UpdateTime(Time.deltaTime);
            _presenter.UpdateScoring();
        }
    }
}
