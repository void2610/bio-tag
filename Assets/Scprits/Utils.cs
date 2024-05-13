using UnityEngine;
using Unity.Netcode;

public class Utils : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab = null;
    void Start()
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
