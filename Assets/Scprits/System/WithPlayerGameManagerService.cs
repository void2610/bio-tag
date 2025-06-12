using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class WithPlayerGameManagerService : IGameManagerService
{
    public int? GameState { get; private set; } = 0;
    public int ItIndex { get; private set; }
    public float LastTagTime { get; private set; }
    public List<float> PlayerScores { get; private set; } = new List<float>();
    public List<string> PlayerNames { get; private set; } = new List<string>();
    
    public event System.Action<int> OnGameStateChanged;
    public event System.Action<int> OnItChanged;
    
    private float _startTime;
    private readonly GameConfig _gameConfig;
    
    [Inject]
    public WithPlayerGameManagerService(GameConfig gameConfig)
    {
        _gameConfig = gameConfig;
    }
    
    public void StartGame()
    {
        SetGameState(1);
        PlayerScores.Clear();
        // 1 main player + N NPCs
        for (var i = 0; i < _gameConfig.npcCount + 1; i++)
        {
            PlayerScores.Add(0);
        }
        ItIndex = Random.Range(0, _gameConfig.npcCount + 1);
        _startTime = Time.time;
        OnItChanged?.Invoke(ItIndex);
    }
    
    public void ChangeIt(int index)
    {
        if (Time.time - LastTagTime > 1 && ItIndex != index && GameState == 1)
        {
            ItIndex = index;
            LastTagTime = Time.time;
            OnItChanged?.Invoke(ItIndex);
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
    
    public float GetGameLength()
    {
        return _gameConfig.gameLength;
    }
    
    public int GetNpcCount()
    {
        return _gameConfig.npcCount;
    }
}