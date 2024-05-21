using UnityEngine;
using Unity.Netcode;

public class Utils : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerPrefab = null;
    public override void OnNetworkSpawn()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (NetworkManager.Singleton == null)
        {
            Instantiate(playerPrefab);
        }
    }

    void Update()
    {

    }
}
