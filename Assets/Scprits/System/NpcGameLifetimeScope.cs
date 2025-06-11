using UnityEngine;
using VContainer;
using VContainer.Unity;

public class NpcGameLifetimeScope : LifetimeScope
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private float gameLength = 60f;
    [SerializeField] private int npcCount = 1;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // 共通サービス
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
        builder.Register<IThemeService, ThemeService>(Lifetime.Singleton);
        
        // NPCゲーム専用サービス
        builder.Register<IGameManagerService, NPCGameManagerService>(Lifetime.Singleton);
        builder.Register<IPlayerSpawnService, PlayerSpawnService>(Lifetime.Singleton);
        builder.Register<IGameUIService, GameUIService>(Lifetime.Singleton);
        
        // 設定値をコンテナに登録
        var gameConfig = new GameConfig
        {
            playerPrefab = this.playerPrefab,
            npcPrefab = this.npcPrefab,
            gameLength = this.gameLength,
            npcCount = this.npcCount
        };
        builder.RegisterInstance(gameConfig);
        
        // ゲーム関連コンポーネント
        builder.RegisterComponentInHierarchy<GameUIToolkit>();
        builder.RegisterComponentInHierarchy<ItMarker>();
        
        // NPCGameManagerをEntryPointとして登録
        builder.RegisterEntryPoint<NpcGameEntryPoint>();
    }
}
