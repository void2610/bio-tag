using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VitalRouter;
using BioTag.GameUI;

[Routes]
public partial class PlayerGameManagerService : IGameManagerService
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
    
    [Inject]
    public PlayerGameManagerService(GameConfig gameConfig)
    {
        _gameConfig = gameConfig;
    }
    
    public void StartGame()
    {
        SetGameState(1);
        ItIndex = Random.Range(0, 2);
        _startTime = Time.time;
        OnItChanged?.Invoke(ItIndex);

        // "It"プレイヤー更新Commandを発行
        var itName = ItIndex >= 0 && ItIndex < PlayerNames.Count ? PlayerNames[ItIndex] : "---";
        Router.Default.PublishAsync(new UpdateItPlayerCommand(ItIndex, itName));
    }
    
    public void ChangeIt(int index)
    {
        if (Time.time - LastTagTime > 1 && ItIndex != index && GameState == 1)
        {
            ItIndex = index;
            LastTagTime = Time.time;
            OnItChanged?.Invoke(ItIndex);

            // "It"プレイヤー更新Commandを発行
            var itName = ItIndex >= 0 && ItIndex < PlayerNames.Count ? PlayerNames[ItIndex] : "---";
            Router.Default.PublishAsync(new UpdateItPlayerCommand(ItIndex, itName));
        }
    }
    
    public void SetGameState(int state)
    {
        if (GameState != state)
        {
            var previousState = GameState;
            GameState = state;
            OnGameStateChanged?.Invoke(state);

            // ゲーム状態変更Commandを発行
            Router.Default.PublishAsync(new GameStateChangedCommand(state, previousState));
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
    /// タグイベントコマンドハンドラ
    /// </summary>
    [Route]
    private void On(PlayerTaggedCommand cmd)
    {
        ChangeIt(cmd.TaggedPlayerIndex);
    }
}