using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VitalRouter;
using BioTag.GameUI;
using BioTag.Biometric;
using TagGame;
using Experiment;

/// <summary>
/// WithNPCシーン用ゲームマネージャーサービス
/// VitalRouter Commandでゲーム状態の変化を通知
/// </summary>
[Routes]
public partial class NPCGameManagerService : IGameManagerService
{
    public int? GameState { get; private set; } = 0;
    public int CurrentItIndex { get; private set; }
    public float LastTagTime { get; private set; }
    public List<float> PlayerScores { get; } = new();
    public List<string> PlayerNames { get; } = new();

    private float _startTime;
    private readonly GameConfig _gameConfig;
    private readonly GsrProcessorService _gsrProcessor;
    private readonly ExperimentSettings _experimentSettings;
    private TagGameDataLogger _dataLogger;
    private IPlayerSpawnService _playerSpawnService;

    // 生体状態（VitalRouterで更新）
    private bool _isExcited = false;

    [Inject]
    public NPCGameManagerService(GameConfig gameConfig, GsrProcessorService gsrProcessor, ExperimentSettings experimentSettings)
    {
        _gameConfig = gameConfig;
        _gsrProcessor = gsrProcessor;
        _experimentSettings = experimentSettings;
    }

    /// <summary>
    /// IPlayerSpawnServiceを設定（プレイヤー位置取得用）
    /// </summary>
    public void SetPlayerSpawnService(IPlayerSpawnService playerSpawnService)
    {
        _playerSpawnService = playerSpawnService;
    }

    public void StartGame()
    {
        SetGameState(1);
        PlayerScores.Clear();
        for (var i = 0; i < _gameConfig.npcCount + 1; i++)
        {
            PlayerScores.Add(0);
        }
        CurrentItIndex = Random.Range(0, _gameConfig.npcCount + 1);
        _startTime = Time.time;

        // "It"プレイヤー更新Commandを発行（Transform無し - ゲーム開始時）
        var itName = CurrentItIndex >= 0 && CurrentItIndex < PlayerNames.Count ? PlayerNames[CurrentItIndex] : "---";
        Router.Default.PublishAsync(new ItChangedCommand(CurrentItIndex, itName, null));

        // ログ記録開始
        if (_experimentSettings != null && _experimentSettings.enableLogging)
        {
            InitializeLogger();
            _dataLogger.RecordGameStart(CurrentItIndex, GetPlayerPositions());
        }
    }

    /// <summary>
    /// ロガーを初期化
    /// </summary>
    private void InitializeLogger()
    {
        _dataLogger = new TagGameDataLogger();

        var sessionInfo = new TagGameSessionInfo
        {
            participantID = _experimentSettings.participantId,
            experimentGroup = _experimentSettings.experimentGroup.ToString(),
            gameMode = GameMode.PlayerVsNPC,
            playerCount = 1,
            npcCount = _gameConfig.npcCount,
            gameLengthSeconds = _gameConfig.gameLength,
            roomTemperature = _experimentSettings.roomTemperature,
            roomHumidity = _experimentSettings.roomHumidity
        };

        _dataLogger.StartSession(sessionInfo);
    }

    /// <summary>
    /// "It"プレイヤー変更（Transformあり）
    /// </summary>
    public void ChangeIt(int index, Transform targetTransform)
    {
        if (!(Time.time - LastTagTime > 1) || CurrentItIndex == index || GameState != 1) return;

        CurrentItIndex = index;
        LastTagTime = Time.time;

        // "It"プレイヤー変更Commandを発行
        var itName = CurrentItIndex >= 0 && CurrentItIndex < PlayerNames.Count ? PlayerNames[CurrentItIndex] : "---";
        Router.Default.PublishAsync(new ItChangedCommand(CurrentItIndex, itName, targetTransform));

        // 鬼交代をログ記録
        if (_dataLogger != null)
        {
            _dataLogger.RecordItChange(CurrentItIndex, GetPlayerPositions(),
                _gsrProcessor.CurrentGsrRaw,
                _gsrProcessor.CurrentGsrDerivative,
                _gsrProcessor.CurrentThreshold,
                GetIsExcited());
        }
    }
    
    public void SetGameState(int state)
    {
        if (GameState != state)
        {
            var previousState = GameState;
            GameState = state;

            // ゲーム状態変更Commandを発行
            Router.Default.PublishAsync(new GameStateChangedCommand(state, previousState));
        }
    }
    
    public void AddPlayerScore(float score)
    {
        PlayerScores.Add(score);
    }
    
    public void AddPlayerName(string name)
    {
        PlayerNames.Add(name);
    }
    
    public void ClearScores()
    {
        PlayerScores.Clear();
    }
    
    public float GetElapsedTime()
    {
        return Time.time - _startTime;
    }

    /// <summary>
    /// ゲーム状態の定期記録（Update()から呼び出す想定）
    /// </summary>
    public void UpdateLogging()
    {
        if (_dataLogger != null && GameState == 1)
        {
            // 1秒ごとに記録（フレームレート非依存）
            if (Time.frameCount % 60 == 0)
            {
                _dataLogger.RecordGameTick(CurrentItIndex, GetPlayerPositions(),
                    _gsrProcessor.CurrentGsrRaw, _gsrProcessor.CurrentGsrDerivative, _gsrProcessor.CurrentThreshold,
                    GetIsExcited());
            }
        }
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    public void EndGame()
    {
        if (_dataLogger != null)
        {
            _dataLogger.RecordGameEnd(PlayerNames, PlayerScores, GetPlayerPositions());
            _dataLogger.Dispose();
        }
    }

    /// <summary>
    /// プレイヤー位置を取得
    /// </summary>
    private List<Vector3> GetPlayerPositions()
    {
        var positions = new List<Vector3>();

        if (_playerSpawnService?.SpawnedPlayers != null)
        {
            foreach (var player in _playerSpawnService.SpawnedPlayers)
            {
                if (player != null)
                    positions.Add(player.transform.position);
            }
        }

        return positions;
    }

    /// <summary>
    /// 興奮状態を取得
    /// </summary>
    private bool GetIsExcited()
    {
        return _isExcited;
    }

    /// <summary>
    /// 生体状態変化コマンドハンドラ
    /// </summary>
    [Route]
    private void On(BiometricStateChangedCommand cmd)
    {
        _isExcited = cmd.NewState == BiometricState.Excited;
    }

    /// <summary>
    /// タグイベントコマンドハンドラ
    /// </summary>
    [Route]
    private void On(PlayerTaggedCommand cmd)
    {
        ChangeIt(cmd.TaggedPlayerIndex, cmd.TaggedPlayerTransform);
    }
}