using System;
using UnityEngine;

public class PlayerDataService : IPlayerDataService
{
    private const string PLAYER_NAME_KEY = "PlayerName";
    private const string DEFAULT_PLAYER_NAME = "No Name";
    
    public string GetPlayerName() => PlayerPrefs.GetString(PLAYER_NAME_KEY, DEFAULT_PLAYER_NAME);
    
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "NoName";
        
        PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
        PlayerPrefs.Save();
    }
}