using System.Collections.Generic;
using UnityEngine;

public interface IGameManagerService
{
    int? GameState { get; }
    int ItIndex { get; }
    float LastTagTime { get; }
    List<float> PlayerScores { get; }
    List<string> PlayerNames { get; }
    
    void StartGame();
    void ChangeIt(int index);
    void SetGameState(int state);
    void AddPlayerScore(float score);
    void AddPlayerName(string name);
    void ClearScores();
    float GetElapsedTime();
    
    event System.Action<int> OnGameStateChanged;
    event System.Action<int> OnItChanged;
}