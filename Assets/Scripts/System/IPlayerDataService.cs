using System;

public interface IPlayerDataService
{
    string GetPlayerName();
    void SetPlayerName(string name);
    event Action<string> PlayerNameChanged;
}