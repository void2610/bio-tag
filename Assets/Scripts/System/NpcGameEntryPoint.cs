using UnityEngine;
using VContainer;
using VContainer.Unity;
using System;
using VitalRouter;
using BioTag.GameUI;

/// <summary>
/// WithNPCシーンのエントリーポイント
/// ゲーム初期化とメインループを管理
/// </summary>
public class NpcGameEntryPoint : IStartable, ITickable
{
    private readonly IGameManagerService _gameManager;
    private readonly IPlayerSpawnService _playerSpawn;
    private readonly IPlayerDataService _playerDataService;
    private readonly IGameUIService _gameUI;
    private readonly GameConfig _gameConfig;

    private bool _isPlayerReady = false;

    [Inject]
    public NpcGameEntryPoint(
        IGameManagerService gameManager,
        IPlayerSpawnService playerSpawn,
        IPlayerDataService playerDataService,
        IGameUIService gameUI,
        GameConfig gameConfig)
    {
        _gameManager = gameManager;
        _playerSpawn = playerSpawn;
        _playerDataService = playerDataService;
        _gameUI = gameUI;
        _gameConfig = gameConfig;
    }

    public void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _gameManager.SetGameState(0);
        _gameUI.ShowStartGame();

        SpawnPlayersAndNpc();
    }

    private void SpawnPlayersAndNpc()
    {
        // プレーヤーを生成
        var playerPosition = _playerSpawn.GetRandomSpawnPosition();
        var player = _playerSpawn.SpawnPlayer(_gameConfig.playerPrefab, playerPosition, 0);
        // NPCを生成
        for (int i = 1; i < _gameConfig.npcCount + 1; i++)
        {
            var npcPosition = _playerSpawn.GetRandomSpawnPosition();
            _playerSpawn.SpawnNpc(_gameConfig.npcPrefab, npcPosition, i, player.transform);
            _gameManager.AddPlayerName($"NPC{i}");
        }
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
            // デバッグ用: Gキーでプレイヤー1を"It"にする
            var targetPlayer = GetPlayerByIndex(1);
            if (targetPlayer != null)
            {
                _gameManager.ChangeIt(1, targetPlayer.transform);
            }
        }

        if (_gameManager.GameState == 0 && Input.GetKeyDown(KeyCode.F))
        {
            _isPlayerReady = true;
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
    
    private void UpdateGameLogic()
    {
        switch (_gameManager.GameState)
        {
            case 0: // Waiting
                if (_isPlayerReady)
                {
                    _gameManager.StartGame();

                    // StartGame直後にItMarkerを初期化するためItChangedCommandを発行
                    var itPlayer = GetPlayerByIndex(_gameManager.CurrentItIndex);
                    if (itPlayer != null)
                    {
                        var itName =  _gameManager.PlayerNames[_gameManager.CurrentItIndex];
                        Router.Default.PublishAsync(new ItChangedCommand(_gameManager.CurrentItIndex, itName, itPlayer.transform));
                    }
                }
                break;
                
            case 1: // Playing
                // スコア更新
                var itPlayerScores = _gameManager.PlayerScores;
                if (_gameManager.CurrentItIndex < itPlayerScores.Count)
                {
                    itPlayerScores[_gameManager.CurrentItIndex] += Time.deltaTime;
                }

                // タイマー更新Commandを発行
                var elapsedTime = _gameManager.GetElapsedTime();
                Router.Default.PublishAsync(new UpdateTimerCommand(elapsedTime));

                // スコアボード更新Commandを発行
                Router.Default.PublishAsync(new UpdateScoreBoardCommand(
                    _gameManager.PlayerNames,
                    _gameManager.PlayerScores
                ));

                // ログ記録を更新
                _gameManager.UpdateLogging();

                // ゲーム終了判定
                if (_gameManager.GetElapsedTime() >= _gameConfig.gameLength)
                {
                    _gameManager.EndGame();
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
}