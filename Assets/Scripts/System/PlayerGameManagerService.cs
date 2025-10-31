using System.Collections.Generic;
using UnityEngine;
using VContainer;
using TagGame;

public class PlayerGameManagerService : IGameManagerService
{
    public int? GameState { get; private set; } = 0;
    public int ItIndex { get; private set; }
    public float LastTagTime { get; private set; }
    public List<float> PlayerScores { get; private set; } = new ();
    public List<string> PlayerNames { get; private set; } = new ();

    public event System.Action<int> OnGameStateChanged;
    public event System.Action<int> OnItChanged;

    private float _startTime;
    private readonly GameConfig _gameConfig;
    private TagGameDataLogger _dataLogger;
    private IPlayerSpawnService _playerSpawnService;

    // ログ設定
    public bool EnableLogging { get; set; } = false;
    public string ParticipantID { get; set; } = "P001";
    public string ExperimentGroup { get; set; } = "BfHuman";
    public string TestType { get; set; } = "Pre";

    [Inject]
    public PlayerGameManagerService(GameConfig gameConfig)
    {
        _gameConfig = gameConfig;
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
        ItIndex = Random.Range(0, 2);
        _startTime = Time.time;
        OnItChanged?.Invoke(ItIndex);

        // ログ記録開始
        if (EnableLogging)
        {
            InitializeLogger();
            _dataLogger.RecordGameStart(ItIndex, GetPlayerPositions());
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
            participantID = ParticipantID,
            experimentGroup = ExperimentGroup,
            testType = TestType,
            gameMode = GameMode.PlayerVsPlayer,
            playerCount = 2,
            npcCount = 0,
            gameLengthSeconds = _gameConfig.gameLength,
            roomTemperature = 23.5f,
            roomHumidity = 45.0f
        };

        _dataLogger.StartSession(sessionInfo);
    }
    
    public void ChangeIt(int index)
    {
        if (Time.time - LastTagTime > 1 && ItIndex != index && GameState == 1)
        {
            ItIndex = index;
            LastTagTime = Time.time;
            OnItChanged?.Invoke(ItIndex);

            // 鬼交代をログ記録
            if (EnableLogging && _dataLogger != null)
            {
                _dataLogger.RecordItChange(ItIndex, GetPlayerPositions(), GetGsrValue(), GetIsExcited());
            }
        }
    }
    
    public void SetGameState(int state)
    {
        if (GameState != state)
        {
            GameState = state;
            OnGameStateChanged?.Invoke(state);
        }
    }
    
    public void AddPlayerName(string name)
    {
        PlayerNames.Add(name);
        PlayerScores.Add(0);
    }
    
    public float GetElapsedTime()
    {
        return Time.time - _startTime;
    }
    
    public float GetGameLength()
    {
        return _gameConfig.gameLength;
    }

    /// <summary>
    /// ゲーム状態の定期記録（Update()から呼び出す想定）
    /// </summary>
    public void UpdateLogging()
    {
        if (EnableLogging && _dataLogger != null && GameState == 1)
        {
            // 1秒ごとに記録（フレームレート非依存）
            if (Time.frameCount % 60 == 0)
            {
                _dataLogger.RecordGameTick(ItIndex, GetPlayerPositions(), GetGsrValue(), GetIsExcited());
            }
        }
    }

    /// <summary>
    /// ゲーム終了処理
    /// </summary>
    public void EndGame()
    {
        if (EnableLogging && _dataLogger != null)
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
    /// GSR値を取得（TODO: GsrGraphから取得）
    /// </summary>
    private float GetGsrValue()
    {
        // TODO: GsrGraph.Instance?.CurrentGsrRaw ?? 0f;
        return 0f;
    }

    /// <summary>
    /// 興奮状態を取得（TODO: GsrGraphから取得）
    /// </summary>
    private bool GetIsExcited()
    {
        // TODO: GsrGraph.Instance?.IsExcited ?? false;
        return false;
    }
}