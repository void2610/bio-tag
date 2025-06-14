using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class NpcGameLifetimeScope : LifetimeScope
{
    [SerializeField] private PlayerNameUI playerNameUIPrefab;
    [SerializeField] private GameConfig gameConfig;
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
}
