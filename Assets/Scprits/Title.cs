using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class Title : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Pending = true;
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
        {
            response.Approved = false;
            response.Pending = false;
            return;
        }

        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;

        var pos = new Vector3(0, 2, 0);
        pos.x += NetworkManager.Singleton.ConnectedClients.Count * 5;
        response.Position = pos;
        response.Rotation = Quaternion.identity;
        response.Pending = false;
    }

    public void Start()
    {
#if UNITY_SERVER
        Debug.Log("Starting server");
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartServer();
#endif
    }
}
