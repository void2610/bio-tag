using System;
using UnityEngine.SceneManagement;

public class SceneService : ISceneService
{
    private const string PLAYER_SCENE_NAME = "WithPlayer";
    private const string NPC_SCENE_NAME = "WithNPC";
    
    public event Action<string> SceneLoadStarted;
    
    public void LoadPlayerScene() => LoadScene(PLAYER_SCENE_NAME);
    public void LoadNpcScene() => LoadScene(NPC_SCENE_NAME);
    
    public void LoadScene(string sceneName)
    {
        SceneLoadStarted?.Invoke(sceneName);
        SceneManager.LoadScene(sceneName);
    }
}