using UnityEngine;
using VContainer;
using VContainer.Unity;
using System;
using VitalRouter;
using BioTag.GameUI;

public class PlayerGameEntryPoint : IStartable, ITickable, IDisposable
{
    private readonly IGameManagerService _gameManager;
    private readonly IPlayerSpawnService _playerSpawn;
    private readonly IPlayerDataService _playerDataService;
    private readonly IGameUIService _gameUI;
    private readonly GameConfig _gameConfig;
    private readonly ItMarker _itMarker;
    
    private bool _isPlayerReady = false;
    
    [Inject]
    public PlayerGameEntryPoint(
        IGameManagerService gameManager,
        IPlayerSpawnService playerSpawn,
        IPlayerDataService playerDataService,
        IGameUIService gameUI,
        GameConfig gameConfig,
        ItMarker itMarker)
    {
        _gameManager = gameManager;
        _playerSpawn = playerSpawn;
        _playerDataService = playerDataService;
        _gameUI = gameUI;
        _gameConfig = gameConfig;
        _itMarker = itMarker;
    }
    
    public void Start()
    {
        InitializeGame();
        SetupEventSubscriptions();
        
        if (Display.displays.Length > 1) Display.displays[1].Activate();
    }
    
    private void InitializeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _gameManager.SetGameState(0);
        _gameUI.ShowStartGame();
        
        SpawnPlayers();
    }
    
    private void SpawnPlayers()
    {
        _playerSpawn.SpawnPlayer(_gameConfig.playerPrefab, _playerSpawn.GetRandomSpawnPosition(), 0);
        _playerSpawn.SpawnPlayer(_gameConfig.subPlayerPrefab, _playerSpawn.GetRandomSpawnPosition(), 1);
    }
    
    private void SetupEventSubscriptions()
    {
        _gameManager.OnItChanged += OnItChanged;
        _gameManager.OnGameStateChanged += OnGameStateChanged;
    }
    
    private void OnItChanged(int newItIndex)
    {
        var targetPlayer = GetPlayerByIndex(newItIndex);
        if (targetPlayer)
        {
            _itMarker.SetTarget(targetPlayer.transform);
        }
    }
    
    private void OnGameStateChanged(int newState)
    {
        switch (newState)
        {
            case 1: // Playing
                _gameUI.ClearMessage();
                break;
            case 2: // Game Over
                _gameUI.ShowGameOver();
                break;
        }
    }
    
    private GameObject GetPlayerByIndex(int index)
    {
        if (index >= 0 && index < _playerSpawn.SpawnedPlayers.Count)
        {
            return _playerSpawn.SpawnedPlayers[index];
        }
        return null;
    }
    
    public void Tick()
    {
        HandleInput();
        UpdateGameLogic();
    }
    
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            _gameManager.ChangeIt(1);
        }
        
        if (_gameManager.GameState == 0 && Input.GetKeyDown(KeyCode.F))
        {
            _isPlayerReady = true;
        }
    }
    
    private void UpdateGameLogic()
    {
        switch (_gameManager.GameState)
        {
            case 0: // Waiting
                if (_isPlayerReady)
                {
                    _gameManager.StartGame();
                }
                break;
                
            case 1: // Playing
                // スコア更新
                var itPlayerScores = _gameManager.PlayerScores;
                if (_gameManager.ItIndex < itPlayerScores.Count)
                {
                    itPlayerScores[_gameManager.ItIndex] += Time.deltaTime;
                }

                // タイマー更新Commandを発行
                var elapsedTime = _gameManager.GetElapsedTime();
                Router.Default.PublishAsync(new UpdateTimerCommand(elapsedTime));

                // ゲーム終了判定
                if (_gameManager is PlayerGameManagerService gameManagerService && _gameManager.GetElapsedTime() >= gameManagerService.GetGameLength())
                {
                    _gameManager.SetGameState(2);
                }

                // プレーヤー間距離をUDPで送信（元の実装を維持）
                SendPlayerDistance();
                break;
        }
    }
    
    private void SendPlayerDistance()
    {
        var players = _playerSpawn.SpawnedPlayers;
        if (players.Count >= 2)
        {
            var distance = Vector3.Distance(
                players[0].transform.position,
                players[1].transform.position
            );
            UDP.instance.SendData(distance);
        }
    }
    
    public void Dispose()
    {
        if (_gameManager != null)
        {
            _gameManager.OnItChanged -= OnItChanged;
            _gameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}