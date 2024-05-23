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

    private Dictionary<ulong, string> playerNames = new Dictionary<ulong, string>();

    public NetworkVariable<int> PlayerCount = new NetworkVariable<int>();
    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            PlayerCount.Value = 0;
        }
        if (IsOwner)
        {
            PlayerCount.Value++;
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
        PlayerCount.Value--;
        // if (playerNames.ContainsKey(clientId))
        // {
        //     playerNames.Remove(clientId);
        // }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(ulong clientId)
    {
        PlayerCount.Value++;
    }

    private void Update()
    {
    }
}
