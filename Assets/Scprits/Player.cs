using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerCameraPrefab = null;
    public AudioClip[] FootstepAudioClips;
    public AudioClip LandingAudioClip;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

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
    private float animationBlend = 0f;

    public override void OnNetworkSpawn()
    {

    }

    void Start()
    {
        animator = GetComponent<Animator>();
        cCon = GetComponent<CharacterController>();
        if (playerCamera == null && NetworkManager.Singleton == null)
        {
            playerCamera = Instantiate(playerCameraPrefab);
            //子オブジェクトを取得
            GameObject cameraRoot = this.transform.Find("PlayerCameraRoot").gameObject;
            playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Follow = cameraRoot.transform;
            playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().LookAt = cameraRoot.transform;
        }
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
                playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Follow = cameraRoot.transform;
                playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().LookAt = cameraRoot.transform;
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
        float targetSpeed = input.normalized.magnitude;
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

            }
            else
            {
                targetSpeed = 0f;
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


        animator.SetFloat(Animator.StringToHash("MotionSpeed"), input.normalized.magnitude);
        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * 10);
        if (animationBlend < 0.01f) animationBlend = 0f;
        animator.SetFloat("Speed", animationBlend);

        velocity.y += Physics.gravity.y * Time.deltaTime;
        cCon.Move(velocity * Time.deltaTime);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (!IsOwner) return;
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(cCon.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (!IsOwner) return;
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(cCon.center), FootstepAudioVolume);
        }
    }
}
