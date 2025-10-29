using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;
using VitalRouter;
using BioTag.Audio;
using BioTag.Biometric;

public class NpcGameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerNameUI playerNameUIPrefab;
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private AudioConfig audioConfig;
    protected override void Configure(IContainerBuilder builder)
    {
        // 共通サービス
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
        builder.Register<IThemeService, ThemeService>(Lifetime.Singleton);
        builder.Register<IPlayerDataService, PlayerDataService>(Lifetime.Singleton);

        // NPCゲーム専用サービス
        builder.Register<IGameManagerService, NPCGameManagerService>(Lifetime.Singleton);
        builder.Register<IPlayerSpawnService, PlayerSpawnService>(Lifetime.Singleton);
        builder.Register<IGameUIService, GameUIService>(Lifetime.Singleton);

        // AudioService (VitalRouter使用)
        builder.RegisterInstance(audioConfig).As<AudioConfig>();
        builder.Register<AudioService>(Lifetime.Singleton);

        // BiometricService (VitalRouter使用)
        builder.Register<BiometricService>(Lifetime.Singleton);

        // 設定値をコンテナに登録
        builder.RegisterInstance(gameConfig);
        builder.RegisterInstance(playerNameUIPrefab).As<PlayerNameUI>();

        // ゲーム関連コンポーネント
        builder.RegisterComponentInHierarchy<GameUIToolkit>();
        builder.RegisterComponentInHierarchy<ItMarker>();
        builder.RegisterComponentInHierarchy<SensorManager>();

        // NPCGameManagerをEntryPointとして登録
        builder.RegisterEntryPoint<NpcGameEntryPoint>();
    }

    protected override void Awake()
    {
        base.Awake();

        // AudioServiceをVitalRouterのデフォルトルーターに登録
        if (Container.TryResolve<AudioService>(out var audioService))
        {
            audioService.MapTo(Router.Default);
        }

        // BiometricServiceをVitalRouterのデフォルトルーターに登録
        if (Container.TryResolve<BiometricService>(out var biometricService))
        {
            biometricService.MapTo(Router.Default);
        }
    }
}
