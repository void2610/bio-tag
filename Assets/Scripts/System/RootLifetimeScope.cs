using UnityEngine;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using BioTag.Biometric;

/// <summary>
/// GSRデータソースの種類
/// </summary>
public enum GsrDataSource
{
    TcpServer,      // ネットワーク経由（Spresenseなど）
    SerialServer,   // USBシリアル通信（Arduino Unoなど）
    Mock            // モックデータ（テスト用）
}

/// <summary>
/// ルートライフタイムスコープ - 全シーンで共有されるサービスを管理
/// GSRデータソース(TcpServer/SerialServer/GsrMock)とBiometricServiceを提供
/// </summary>
public class RootLifetimeScope : LifetimeScope
{
    [Header("GSRデータソース設定")]
    [SerializeField] private GsrDataSource dataSource = GsrDataSource.SerialServer;

    [Header("シリアル通信設定（dataSource = SerialServer時に使用）")]
    [SerializeField] private string serialPortName = "/dev/cu.usbmodem14201";
    [SerializeField] private int serialBaudRate = 9600;

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

        // GSRデータソース (TcpServer / SerialServer / GsrMock)
        switch (dataSource)
        {
            case GsrDataSource.TcpServer:
                builder.Register<TcpServer>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
                Debug.Log("[RootLifetimeScope] TcpServerを使用");
                break;

            case GsrDataSource.SerialServer:
                builder.Register(_ => new SerialServer(serialPortName, serialBaudRate), Lifetime.Singleton)
                    .AsImplementedInterfaces()
                    .AsSelf();
                Debug.Log($"[RootLifetimeScope] SerialServerを使用 (Port: {serialPortName}, Baud: {serialBaudRate})");
                break;

            case GsrDataSource.Mock:
                builder.Register<GsrMock>(Lifetime.Singleton).AsImplementedInterfaces();
                Debug.Log("[RootLifetimeScope] GsrMockを使用");
                break;
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
