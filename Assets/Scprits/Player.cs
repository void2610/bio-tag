using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private float speed = 1;

    private Rigidbody rb;
    private Vector2 moveInput = Vector2.zero;
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (IsOwner)
        {
            SetMoveInputServerRpc(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
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
        rb.AddForce(velocity);
    }
}
