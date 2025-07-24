using UnityEngine;

public interface IGameUIService
{
    void SetMessage(string message, GameUIToolkit.MessageType messageType);
    void ClearMessage();
    void UpdateTimer(float timeRemaining);
    void ShowStartGame();
    void ShowGameOver();
}