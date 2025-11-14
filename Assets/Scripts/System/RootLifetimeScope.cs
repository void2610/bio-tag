using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using BioTag.Biometric;

/// <summary>
/// ルートライフタイムスコープ - 全シーンで共有されるサービスを管理
/// GSRデータソース(TcpServer/GsrMock)とBiometricServiceを提供
/// </summary>
public class RootLifetimeScope : LifetimeScope
{
    [Header("GSRデータソース設定")]
    [SerializeField] private bool useTcpServer = true;

    [Header("GSRプロセッサ設定")]
    [SerializeField] private int historyLength = 500;
    [SerializeField] private int filterWindowSize = 10;
    [SerializeField] private float threshold = 5f;
    [SerializeField] private float thresholdMagnification = 1.5f;
    [SerializeField] private float checkLength = 0.1f;

    protected override void Configure(IContainerBuilder builder)
    {
        // GsrProcessorService (GSRデータ処理層)
        builder.Register(_ =>
            new GsrProcessorService(
                historyLength,
                filterWindowSize,
                threshold,
                thresholdMagnification,
                checkLength),
            Lifetime.Singleton);

        // GSRデータソース (TcpServer or GsrMock)
        if (useTcpServer)
        {
            builder.Register<TcpServer>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            Debug.Log("[RootLifetimeScope] TcpServerを使用");
        }
        else
        {
            builder.Register<GsrMock>(Lifetime.Singleton).AsImplementedInterfaces();
            Debug.Log("[RootLifetimeScope] GsrMockを使用");
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // シーン間で破棄されないようにする
        DontDestroyOnLoad(gameObject);

        // BiometricServiceをVitalRouterのデフォルトルーターに登録
        if (Container.TryResolve<BiometricService>(out var biometricService))
        {
            biometricService.MapTo(Router.Default);
        }

        // GsrProcessorServiceをVitalRouterのデフォルトルーターに登録
        if (Container.TryResolve<GsrProcessorService>(out var gsrProcessor))
        {
            gsrProcessor.MapTo(Router.Default);
        }
    }
}
