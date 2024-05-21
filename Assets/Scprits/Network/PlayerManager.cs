using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }
    private void Awake()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Destroy(this.gameObject);
            return;
        }
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public void SetPlayerName(ulong clientId, string name)
    {
        if (IsServer)
        {
            playerNames[clientId] = name;
        }
    }

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
        if (playerNames.ContainsKey(clientId))
        {
            playerNames.Remove(clientId);
        }
    }

    private void Update()
    {
    }
}
