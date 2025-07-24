using UnityEngine;
using VContainer;
using VContainer.Unity;

public class PlayerGameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerNameUI playerNameUIPrefab;
    [SerializeField] private GameConfig gameConfig;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // 共通サービス
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
        builder.Register<IThemeService, ThemeService>(Lifetime.Singleton);
        builder.Register<IPlayerDataService, PlayerDataService>(Lifetime.Singleton);
        
        // WithPlayer専用サービス
        builder.Register<IGameManagerService, PlayerGameManagerService>(Lifetime.Singleton);
        builder.Register<IPlayerSpawnService, PlayerSpawnService>(Lifetime.Singleton);
        builder.Register<IGameUIService, GameUIService>(Lifetime.Singleton);
        
        // 設定値をコンテナに登録
        builder.RegisterInstance(gameConfig).As<GameConfig>();
        builder.RegisterInstance(playerNameUIPrefab).As<PlayerNameUI>();
        
        // ゲーム関連コンポーネント
        builder.RegisterComponentInHierarchy<GameUIToolkit>();
        builder.RegisterComponentInHierarchy<ItMarker>();
        builder.RegisterComponentInHierarchy<SensorManager>();
        
        // WithPlayerEntryPointをEntryPointとして登録
        builder.RegisterEntryPoint<PlayerGameEntryPoint>();
    }
}
