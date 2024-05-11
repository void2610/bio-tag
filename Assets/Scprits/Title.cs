using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class Title : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
