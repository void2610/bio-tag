using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VitalRouter;
using BioTag.GameUI;

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

    [Inject]
    public NPCGameManagerService(GameConfig gameConfig)
    {
        _gameConfig = gameConfig;
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
    /// タグイベントコマンドハンドラ
    /// </summary>
    [Route]
    private void On(PlayerTaggedCommand cmd)
    {
        ChangeIt(cmd.TaggedPlayerIndex, cmd.TaggedPlayerTransform);
    }
}