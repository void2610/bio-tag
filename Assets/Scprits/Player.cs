using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private GameObject playerCameraPrefab = null;
    public AudioClip[] FootstepAudioClips;
    public AudioClip LandingAudioClip;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
    [SerializeField]
    private float jumpPower = 6f;
    [SerializeField]
    private float walkSpeed = 6f;
    private Animator animator;
    private CharacterController cCon;
    private Vector3 velocity = Vector3.zero;
    private GameObject playerCamera = null;
    private float animationBlend = 0f;
    private Vector3 lookDirection = Vector3.zero;
    private Vector3 networkPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        cCon = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (IsOwner)
        {
            AlignPlayerWithCamera();
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

        if (playerCamera != null)
        {
            UpdateCharacterController(movement, playerCamera.transform.forward, Input.GetButtonDown("Jump"));
            AlignPlayerWithCamera();
        }
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

    private void UpdateCharacterController(Vector3 input, Vector3 playerDirection, bool isJump)
    {
        if (cCon.isGrounded)
        {
            velocity = Vector3.zero;
            animator.SetBool("Grounded", true);
            animator.SetBool("FreeFall", false);
            animator.SetBool("Jump", false);


            if (input.magnitude > 0f)
            {
                velocity += transform.forward * input.z * walkSpeed;
                velocity += transform.right * input.x * walkSpeed;
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

        animationBlend = Mathf.Lerp(animationBlend, input.magnitude > 0f ? input.normalized.magnitude : 0, Time.deltaTime * 10);
        animator.SetFloat("Speed", animationBlend < 0.01f ? 0 : animationBlend);
        animator.SetFloat("MotionSpeed", input.normalized.magnitude);


        velocity.y += Physics.gravity.y * Time.deltaTime;
        cCon.Move(velocity * Time.deltaTime);
        if (new Vector3(playerDirection.x, 0, playerDirection.z).magnitude > 0.1f)
        {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z)), Time.deltaTime * 10);
        }
    }

    private void AlignPlayerWithCamera()
    {
        if (playerCamera != null)
        {
            lookDirection = playerCamera.transform.forward;
        }
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
