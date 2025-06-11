using VContainer;

public class GameUIService : IGameUIService
{
    private GameUIToolkit _gameUIToolkit;
    
    [Inject]
    public void Construct(GameUIToolkit gameUIToolkit)
    {
        _gameUIToolkit = gameUIToolkit;
    }
    
    public void SetMessage(string message, GameUIToolkit.MessageType messageType)
    {
        _gameUIToolkit?.SetMessage(message, messageType);
    }
    
    public void ClearMessage() => _gameUIToolkit?.ClearMessage();
    public void ShowStartGame() => SetMessage("Press F to start the game", GameUIToolkit.MessageType.Info);
    public void ShowGameOver() => SetMessage("Game Over", GameUIToolkit.MessageType.Info);
    
    public void UpdateTimer(float timeRemaining)
    {
        // タイマー更新の実装
    }
}