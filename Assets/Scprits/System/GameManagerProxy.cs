using VContainer;

/// <summary>
/// VContainerシステムと既存のGameManagerBase.Instanceシステムの橋渡しを行うプロキシ
/// </summary>
public class GameManagerProxy : GameManagerBase
{
    private IGameManagerService _gameManagerService;
    
    [Inject]
    public void Construct(IGameManagerService gameManagerService)
    {
        _gameManagerService = gameManagerService;
        
        // イベント購読
        _gameManagerService.OnItChanged += (index) => itIndex = index;
        _gameManagerService.OnGameStateChanged += (state) => GameState = state;
    }
    
    public override void ChangeIt(int index)
    {
        _gameManagerService?.ChangeIt(index);
    }
    
    public override void StartGame()
    {
        _gameManagerService?.StartGame();
    }
}