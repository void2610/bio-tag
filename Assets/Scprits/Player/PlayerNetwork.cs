using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField]
    private GameObject gameManagerPrefab;
    [SerializeField]
    private GameObject worldSpaceCanvasPrefab;
    [SerializeField]
    private GameObject playerNameUIPrefab;
    [SerializeField]
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        playerName.Value = PlayerPrefs.GetString("PlayerName", "Player");
        Debug.Log("PlayerName: " + playerName.Value + " OwnerClientId: " + OwnerClientId);
        PlayerManager.Instance?.AddPlayer(OwnerClientId, "Player " + OwnerClientId);


        GameObject canvas = GameObject.Find("WorldSpaceCanvas");
        if (canvas == null)
        {
            canvas = Instantiate(worldSpaceCanvasPrefab);
            canvas.name = "WorldSpaceCanvas";
        }
        GameObject tmp = Instantiate(playerNameUIPrefab, canvas.transform);
        tmp.GetComponent<PlayerNameUI>().SetTargetPlayer(this.gameObject, "Player " + OwnerClientId);
    }
    void Awake()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Instantiate(gameManagerPrefab).GetComponent<NetworkObject>().Spawn();
        }
    }
}
