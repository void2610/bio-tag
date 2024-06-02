using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public static PlayerManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    public int PlayerCount = 0;

    public string GetPlayerName(ulong clientId)
    {
        if (playerNames.TryGetValue(clientId, out var name))
        {
            return name;
        }
        return "Unknown";
    }

    public int GetPlayerCount()
    {
        return playerNames.Count;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        PlayerCount--;
        if (playerNames.ContainsKey(clientId))
        {
            playerNames.Remove(clientId);
        }
    }

    public void AddPlayer(ulong clientId, string name)
    {
        foreach (var item in playerNames)
        {
            Debug.Log(item.Key + " : " + item.Value);
        }
    }
}
