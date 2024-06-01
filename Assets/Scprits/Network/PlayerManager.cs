using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerManager : NetworkBehaviour
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
    public override void OnNetworkSpawn()
    {
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
        PlayerCount--;
        if (playerNames.ContainsKey(clientId))
        {
            playerNames.Remove(clientId);
        }
    }

    public void AddPlayer(ulong clientId, string name)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            playerNames[clientId] = name;
            PlayerCount++;
            AddPlayerClientRpc(clientId, name);
        }
        else
        {
            AddPlayerServerRpc(clientId, name);
        }

        foreach (var item in playerNames)
        {
            Debug.Log(item.Key + " : " + item.Value);
        }
    }

    [ClientRpc]
    public void AddPlayerClientRpc(ulong clientId, string name)
    {
        playerNames[clientId] = name;
        PlayerCount++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(ulong clientId, string name)
    {
        playerNames[clientId] = name;
        PlayerCount++;
        AddPlayerClientRpc(clientId, name);
    }


}
