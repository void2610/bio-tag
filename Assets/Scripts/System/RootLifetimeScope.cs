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

    protected override void Configure(IContainerBuilder builder)
    {
        // BiometricService (VitalRouter使用)
        builder.Register<BiometricService>(Lifetime.Singleton);

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
    }
}
