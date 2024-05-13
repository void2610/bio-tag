using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerCameraPrefab = null;

    private Animator animator;
    private CharacterController cCon;
    private Vector3 velocity = Vector3.zero;
    private GameObject playerCamera = null;
    [SerializeField]
    private float jumpPower = 5f;

    [SerializeField]
    private float walkSpeed = 4f;
    private Vector3 input;
    private bool isJump = false;

    public override void OnNetworkSpawn()
    {

    }

    void Start()
    {
        animator = GetComponent<Animator>();
        cCon = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (IsOwner)
        {
            SetMoveInputServerRpc(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), Input.GetButtonDown("Jump"));

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
    private void SetMoveInputServerRpc(float x, float y, bool j)
    {
        input = new Vector3(x, 0f, y);
        isJump = j;
    }

    private void ServerUpdate()
    {
        UpdateCharacterController();
    }

    private void UpdateCharacterController()
    {
        if (cCon.isGrounded)
        {
            velocity = Vector3.zero;
            //　着地していたらアニメーションパラメータと２段階ジャンプフラグをfalse
            animator.SetBool("Grounded", true);
            animator.SetBool("FreeFall", false);
            animator.SetBool("Jump", false);

            //　方向キーが多少押されている
            if (input.magnitude > 0f)
            {
                velocity += input.normalized * walkSpeed;
                animator.SetFloat("Speed", 1);
            }
            else
            {
                animator.SetFloat("Speed", 0);
            }

            if (isJump)
            {
                animator.SetBool("Jump", true);
                velocity.y += jumpPower;
            }
        }
        else
        {
            animator.SetBool("Grounded", false);
            animator.SetBool("FreeFall", true);
            animator.SetFloat("Speed", 0);
        }


        animator.SetFloat(Animator.StringToHash("MotionSpeed"), input.magnitude * 2);


        velocity.y += Physics.gravity.y * Time.deltaTime;
        cCon.Move(velocity * Time.deltaTime);
    }
}
