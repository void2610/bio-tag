using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using BioTag.Camera;
using ControlTask;
using Experiment;

/// <summary>
/// ControlTaskシーンのVContainer LifetimeScope
/// </summary>
public class ControlTaskLifetimeScope : LifetimeScope
{
    [Header("実験設定")]
    [SerializeField] private ExperimentConfig experimentConfig = new ExperimentConfig();
    [SerializeField] private ExperimentSettings experimentSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        // ExperimentConfigをインスタンス登録
        builder.RegisterInstance(experimentConfig);

        // ExperimentSettingsをインスタンス登録
        if (experimentSettings != null)
        {
            builder.RegisterInstance(experimentSettings);
        }

        // Model (ExperimentConfigから設定を注入)
        builder.Register<ControlTaskModel>(Lifetime.Singleton).AsSelf();

        // Service
        builder.Register<IControlTaskService>(container =>
        {
            var settings = container.TryResolve<ExperimentSettings>(out var s) ? s : null;
            var service = new ControlTaskService();
            service.EnableLogging = settings?.enableLogging ?? true;
            return service;
        }, Lifetime.Singleton);

        builder.Register<ControlTaskPresenter>(Lifetime.Singleton);
        builder.Register<CameraEffectService>(Lifetime.Singleton);

        // UI要素をシーンから取得して登録
        builder.RegisterComponentInHierarchy<TargetStateUI>();
        builder.RegisterComponentInHierarchy<TimerUI>();
        builder.RegisterComponentInHierarchy<ScoreUI>();
        builder.RegisterComponentInHierarchy<GsrGraphView>();

        // EntryPoint
        builder.RegisterEntryPoint<ControlTaskEntryPoint>();
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
