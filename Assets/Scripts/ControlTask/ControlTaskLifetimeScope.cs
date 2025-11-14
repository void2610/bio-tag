using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using BioTag.Camera;
using ControlTask;

/// <summary>
/// ControlTaskシーンのVContainer LifetimeScope
/// </summary>
public class ControlTaskLifetimeScope : LifetimeScope
{
    [Header("実験設定")]
    [SerializeField] private ExperimentConfig experimentConfig = new ExperimentConfig();

    [Header("ログ設定")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private string participantId = "P001";
    [SerializeField] private ExperimentGroup experimentGroup = ExperimentGroup.BfHuman;
    [SerializeField] private TestType testType = TestType.Pre;
    [SerializeField] private float roomTemperature = 23.5f;
    [SerializeField] private float roomHumidity = 45.0f;

    [Header("キャリブレーション設定")]
    [SerializeField] private float baselineGsr = 2.45f;
    [SerializeField] private float minGsr = 1.82f;
    [SerializeField] private float maxGsr = 4.31f;

    protected override void Configure(IContainerBuilder builder)
    {
        // ExperimentConfigをインスタンス登録
        builder.RegisterInstance(experimentConfig);

        // Model (ExperimentConfigから設定を注入)
        builder.Register<ControlTaskModel>(Lifetime.Singleton).AsSelf();

        // Service
        builder.Register<IControlTaskService>(_ =>
        {
            var service = new ControlTaskService();
            service.EnableLogging = enableLogging;
            return service;
        }, Lifetime.Singleton);

        builder.Register<ControlTaskPresenter>(Lifetime.Singleton);
        builder.Register<CameraEffectService>(Lifetime.Singleton);

        // UI要素をシーンから取得して登録
        builder.RegisterComponentInHierarchy<TargetStateUI>();
        builder.RegisterComponentInHierarchy<TimerUI>();
        builder.RegisterComponentInHierarchy<ScoreUI>();
        builder.RegisterComponentInHierarchy<GraphParticleView>();
        builder.RegisterComponentInHierarchy<GsrGraphView>();

        // EntryPoint
        builder.RegisterEntryPoint<ControlTaskEntryPoint>()
            .WithParameter("participantId", participantId)
            .WithParameter("experimentGroup", experimentGroup)
            .WithParameter("testType", testType)
            .WithParameter("roomTemperature", roomTemperature)
            .WithParameter("roomHumidity", roomHumidity)
            .WithParameter("baselineGsr", baselineGsr)
            .WithParameter("minGsr", minGsr)
            .WithParameter("maxGsr", maxGsr);
    }

    protected override void Awake()
    {
        base.Awake();

        // PresenterをVitalRouterのデフォルトルーターに登録
        if (Container.TryResolve<ControlTaskPresenter>(out var presenter))
        {
            presenter.MapTo(Router.Default);
        }
    }
}
