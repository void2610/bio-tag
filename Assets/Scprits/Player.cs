using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerCameraPrefab = null;

    private Rigidbody rb;
    private GameObject playerCamera = null;
    private Vector2 moveInput = Vector2.zero;
    private float speed = 5.0f;

    public override void OnNetworkSpawn()
    {

    }

    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (IsOwner)
        {
            SetMoveInputServerRpc(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            if (playerCamera == null)
            {
                playerCamera = Instantiate(playerCameraPrefab);
                //子オブジェクトを取得
                GameObject cameraRoot = this.transform.Find("PlayerCameraRoot").gameObject;
                playerCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = cameraRoot.transform;
            }
        }
        if (IsServer)
        {
            ServerUpdate();
        }
    }


    [ServerRpc]
    private void SetMoveInputServerRpc(float x, float y)
    {
        moveInput = new Vector2(x, y);
    }

    private void ServerUpdate()
    {
        var velocity = Vector3.zero;
        velocity.x = speed * moveInput.normalized.x;
        velocity.z = speed * moveInput.normalized.y;
        this.transform.Translate(velocity * Time.deltaTime);
    }
}
