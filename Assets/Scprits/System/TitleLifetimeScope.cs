using UnityEngine;
using VContainer;
using VContainer.Unity;

public class TitleLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IPlayerDataService, PlayerDataService>(Lifetime.Singleton);
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
        builder.Register<IThemeService, ThemeService>(Lifetime.Singleton);
        
        builder.RegisterComponentInHierarchy<TitleUIToolkit>();
    }
}