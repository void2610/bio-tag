using UnityEngine;
using Unity.Netcode;

public class TestTransform : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerCameraPrefab = null;
    public float speed = 5.0f;
    private Vector3 networkPosition;
    private GameObject playerCamera = null;

    private void Start()
    {

    }
    private void Update()
    {
        if (IsOwner)
        {
            LocalMoving();
            SentPositionToServerRpc(this.transform.position);

            if (playerCamera == null)
            {
                playerCamera = Instantiate(playerCameraPrefab);
                playerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
            }
        }
        else
        {
            this.transform.position = networkPosition;
        }
    }

    private void LocalMoving()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput);
        this.transform.Translate(movement * speed * Time.deltaTime);
    }

    [ServerRpc]
    private void SentPositionToServerRpc(Vector3 position)
    {
        SentPositionFromClientRpc(position);
    }

    [ClientRpc]
    private void SentPositionFromClientRpc(Vector3 position)
    {
        if (IsOwner)
            return;

        networkPosition = position;
    }
}
