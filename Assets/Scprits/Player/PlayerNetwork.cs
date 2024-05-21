using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork Instance { get; private set; }

    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();

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

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerName.OnValueChanged += OnPlayerNameChanged;
        }

        if (IsServer)
        {
            playerName.OnValueChanged += (oldName, newName) =>
            {
                PlayerManager.Instance.SetPlayerName(OwnerClientId, newName.ToString());
            };
        }
    }

    private void OnPlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        Debug.Log($"Player name changed from {oldName} to {newName}");
    }

    public void SetPlayerName(string newName)
    {
        if (IsOwner)
        {
            playerName.Value = new FixedString32Bytes(newName);
        }
    }

    public string GetPlayerName()
    {
        return playerName.Value.ToString();
    }
}
