using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームマネージャーサービスのインターフェース
/// ゲーム状態管理、スコア管理、"It"プレイヤー管理を提供
/// 状態変化はVitalRouter Commandで通知
/// </summary>
public interface IGameManagerService
{
    int? GameState { get; }
    int CurrentItIndex { get; }
    float LastTagTime { get; }
    List<float> PlayerScores { get; }
    List<string> PlayerNames { get; }

    void StartGame();
    void ChangeIt(int index, Transform targetTransform);
    void SetGameState(int state);
    void AddPlayerName(string name);
    float GetElapsedTime();
}