using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private GameObject gameManagerPrefab;
    [SerializeField]
    private GameObject playerCameraPrefab;
    [SerializeField]
    private AudioClip[] FootstepAudioClips;
    [SerializeField]
    private AudioClip LandingAudioClip;
    [SerializeField]
    private float jumpPower = 6f;
    [SerializeField]
    private float walkSpeed = 6f;
    [Range(0, 1)]
    public float FootstepAudioVolume = 0.5f;
    private Animator animator;
    private CharacterController cCon;
    private GameObject playerCamera = null;
    private float animationBlend = 0f;
    private Vector3 lookDirection = Vector3.zero;
    private Vector3 networkPosition;
    private Vector3 velocity = Vector3.zero;
    private float onLandTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        cCon = GetComponent<CharacterController>();

        if (IsOwner)
        {
            var gm = Instantiate(gameManagerPrefab);
            gm.GetComponent<NetworkObject>().Spawn();
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            if (playerCamera == null)
            {
                playerCamera = Instantiate(playerCameraPrefab);
                playerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
            }

            LocalMoving();
            SentPositionToServerRpc(this.transform.position);
        }
        else
        {
            this.transform.position = networkPosition;
        }
    }

    private void LocalMoving()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        UpdateCharacterController(movement, playerCamera.transform.forward, Input.GetButtonDown("Jump"));
        lookDirection = playerCamera.transform.forward;
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

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (!IsOwner) return;
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var index = Random.Range(0, FootstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(cCon.center), FootstepAudioVolume);
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (!IsOwner) return;
        if (animationEvent.animatorClipInfo.weight > 0.2f && Time.time - onLandTime > 0.1f)
        {
            onLandTime = Time.time;
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(cCon.center), FootstepAudioVolume);
        }
    }
}
