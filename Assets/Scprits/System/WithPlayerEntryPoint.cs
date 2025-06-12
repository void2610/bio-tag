using UnityEngine;
using VContainer;
using VContainer.Unity;
using System;

public class WithPlayerEntryPoint : IStartable, ITickable, IDisposable
{
    private readonly IGameManagerService _gameManager;
    private readonly IPlayerSpawnService _playerSpawn;
    private readonly IPlayerDataService _playerDataService;
    private readonly IGameUIService _gameUI;
    private readonly GameConfig _gameConfig;
    private readonly ItMarker _itMarker;
    
    private bool _isPlayerReady = false;
    
    [Inject]
    public WithPlayerEntryPoint(
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
        SetupLegacyCompatibility();
        InitializeGame();
        SetupEventSubscriptions();
        SetupDisplays();
    }
    
    private void SetupLegacyCompatibility()
    {
        // GameManagerBase.Instanceとの互換性のためプロキシを作成
        if (!GameManagerBase.Instance)
        {
            var proxyGameObject = new GameObject("GameManagerProxy");
            var proxy = proxyGameObject.AddComponent<GameManagerProxy>();
            
            // DIコンテナから依存注入
            var container = VContainer.Unity.LifetimeScope.Find<WithPlayerLifetimeScope>();
            if (container)
            {
                container.Container.Inject(proxy);
            }
        }
    }
    
    private void InitializeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _gameManager.SetGameState(0);
        _gameUI.ShowStartGame();
        
        SpawnPlayersAndNpcs();
    }
    
    private void SpawnPlayersAndNpcs()
    {
        // MainPlayer (人間プレーヤー)を生成
        var mainPlayerPosition = _playerSpawn.GetRandomSpawnPosition();
        var mainPlayer = _playerSpawn.SpawnPlayer(_gameConfig.playerPrefab, mainPlayerPosition, 0);
        var playerName = _playerDataService.GetPlayerName();
        _gameManager.AddPlayerName(playerName);
        
        // SubPlayer NPCsを生成
        for (int i = 1; i < _gameConfig.npcCount + 1; i++)
        {
            var npcPosition = _playerSpawn.GetRandomSpawnPosition();
            _playerSpawn.SpawnPlayer(_gameConfig.subPlayerPrefab, npcPosition, i);
            _gameManager.AddPlayerName($"Player{i}");
        }
    }
    
    private void SetupDisplays()
    {
        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] は主要なデフォルトのディスプレイで、常にオンです。ですから、インデックス 1 から始まります。
        for (var i = 1; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }
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
                
                // ゲーム終了判定
                var gameManagerService = _gameManager as WithPlayerGameManagerService;
                if (gameManagerService != null && _gameManager.GetElapsedTime() >= gameManagerService.GetGameLength())
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