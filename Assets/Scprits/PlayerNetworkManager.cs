using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetworkManager : NetworkBehaviour
{
    public static PlayerNetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerName.OnValueChanged += OnPlayerNameChanged;
        }
    }

    private void OnPlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        // プレイヤー名が変更された際の処理を追加
        Debug.Log($"Player name changed from {oldName} to {newName}");
    }

    public void SetPlayerName(string newName)
    {

        if (IsOwner)
        {
            Debug.Log("PlayerName: " + newName);
            playerName.Value = new FixedString32Bytes(newName);
        }
    }

    public string GetPlayerName()
    {
        return playerName.Value.ToString();
    }
}
