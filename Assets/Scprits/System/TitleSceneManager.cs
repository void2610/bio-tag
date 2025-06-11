using UnityEngine;
using VContainer;
using VContainer.Unity;

public class TitleSceneManager : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IPlayerDataService, PlayerDataService>(Lifetime.Singleton);
        builder.Register<ISceneService, SceneService>(Lifetime.Singleton);
        
        builder.RegisterComponentInHierarchy<TitleUIToolkit>();
    }
}