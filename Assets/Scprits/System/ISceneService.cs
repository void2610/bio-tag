using System;

public interface ISceneService
{
    void LoadScene(string sceneName);
    void LoadPlayerScene();
    void LoadNpcScene();
    event Action<string> SceneLoadStarted;
}