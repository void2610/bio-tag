using UnityEngine;
using Photon.Pun;

public class PlayerBase : MonoBehaviourPunCallbacks
{
    [SerializeField]
    protected GameObject playerCameraPrefab;
    [SerializeField]
    protected GameObject playerNameUIPrefab;
    [SerializeField]
    protected AudioClip[] FootstepAudioClips;
    [SerializeField]
    protected AudioClip LandingAudioClip;
    [SerializeField]
    protected float jumpPower = 6f;
    [SerializeField]
    protected float walkSpeed = 6f;
    [Range(0, 1)]
    protected float FootstepAudioVolume = 0.5f;
    protected Animator animator => GetComponent<Animator>();
    protected CharacterController cCon => GetComponent<CharacterController>();
    protected Rigidbody rb => GetComponent<Rigidbody>();
    protected GameObject playerCamera = null;
    protected float animationBlend = 0f;
    protected Vector3 lookDirection = Vector3.zero;
    protected float onLandTime = 0f;
    protected bool isMovable = true;
    public bool isGrounded = false;
    public Vector3 velocity = Vector3.zero;

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        var canvas = GameObject.Find("WorldSpaceCanvas");
        var ui = Instantiate(playerNameUIPrefab, canvas.transform);
        ui.GetComponent<PlayerNameUI>().SetTargetPlayer(this.gameObject, PlayerPrefs.GetString("PlayerName", "No Name"));
    }

    protected virtual void Update()
    {
    }

    protected virtual void LocalMoving()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        UpdateCharacterController(movement, playerCamera.transform.forward, Input.GetButtonDown("Jump"));
        lookDirection = playerCamera.transform.forward;
    }

    protected virtual void UpdateCharacterController(Vector3 input, Vector3 playerDirection, bool isJump)
    {
        if (!isMovable)
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
            rb.linearVelocity = velocity;
            return;
        }

        if (isGrounded)
        {
            velocity = new Vector3(0, rb.linearVelocity.y, 0);
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
                isGrounded = false;
                velocity.y = jumpPower;
            }
        }
        else
        {
            animator.SetBool("Grounded", false);
            animator.SetBool("FreeFall", true);
            animator.SetFloat("Speed", 0);
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }

        animationBlend = Mathf.Lerp(animationBlend, input.magnitude > 0f ? input.normalized.magnitude : 0, Time.deltaTime * 10);
        animator.SetFloat("Speed", animationBlend < 0.01f ? 0 : animationBlend);
        animator.SetFloat("MotionSpeed", input.normalized.magnitude);


        rb.linearVelocity = velocity;
        if (new Vector3(playerDirection.x, 0, playerDirection.z).magnitude > 0.1f)
        {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z)), Time.deltaTime * 10);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }


    protected virtual void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var index = Random.Range(0, FootstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], this.transform.position, FootstepAudioVolume);
        }
    }
    protected virtual void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.2f && Time.time - onLandTime > 0.1f)
        {
            onLandTime = Time.time;
            AudioSource.PlayClipAtPoint(LandingAudioClip, this.transform.position, FootstepAudioVolume);
        }
    }
}
