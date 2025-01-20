using UnityEngine;
using UnityEngine.VFX;
using Photon.Pun;
using UnityEngine.Serialization;

public class PlayerBase : MonoBehaviourPunCallbacks
{
    [SerializeField] protected GameObject playerCameraPrefab;
    [SerializeField] protected GameObject playerNameUIPrefab;
    [SerializeField] protected AudioClip[] footstepAudioClips;
    [SerializeField] protected AudioClip landingAudioClip;
    [SerializeField] protected float jumpPower = 6f;
    [SerializeField] protected float walkSpeed = 6f;
    public Vector3 velocity = Vector3.zero;
    
    protected Animator Animator => GetComponent<Animator>();
    protected CharacterController CCon => GetComponent<CharacterController>();
    protected GameObject PlayerCamera = null;
    protected float AnimationBlend = 0f;
    protected Vector3 LookDirection = Vector3.zero;
    protected float OnLandTime = 0f;
    protected bool isMovable = true;
    protected VisualEffect ItEffect;
    private const float FOOTSTEP_AUDIO_VOLUME = 0.5f;

    public int index = -1;

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        var canvas = GameObject.Find("WorldSpaceCanvas");
        var ui = Instantiate(playerNameUIPrefab, canvas.transform);
        ui.GetComponent<PlayerNameUI>().SetTargetPlayer(this.gameObject, PlayerPrefs.GetString("PlayerName", "No Name"));
        ItEffect = transform.Find("ItEffect").GetComponent<VisualEffect>();
    }

    protected virtual void Update()
    {
        walkSpeed = GSRGraph.instance.isExcited ? 4f : 6.5f;
    }

    protected virtual void LocalMoving()
    {
        var movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        UpdateCharacterController(movement, PlayerCamera.transform.forward, Input.GetButtonDown("Jump"));
        LookDirection = PlayerCamera.transform.forward;
    }

    protected virtual void UpdateCharacterController(Vector3 input, Vector3 playerDirection, bool isJump)
    {
        if (!isMovable)
        {
            velocity.y += Physics.gravity.y * Time.deltaTime;
            CCon.Move(velocity * Time.deltaTime);
            return;
        }

        if (CCon.isGrounded)
        {
            velocity = Vector3.zero;
            Animator.SetBool("Grounded", true);
            Animator.SetBool("FreeFall", false);
            Animator.SetBool("Jump", false);


            if (input.magnitude > 0f)
            {
                velocity += transform.forward * (input.z * walkSpeed);
                velocity += transform.right * (input.x * walkSpeed);
            }
            if (isJump)
            {
                Animator.SetBool("Jump", true);
                velocity.y += jumpPower;
            }
        }
        else
        {
            Animator.SetBool("Grounded", false);
            // animator.SetBool("FreeFall", true);
            Animator.SetFloat("Speed", 0);
        }

        AnimationBlend = Mathf.Lerp(AnimationBlend, input.magnitude > 0f ? input.normalized.magnitude : 0, Time.deltaTime * 10);
        Animator.SetFloat("Speed", AnimationBlend < 0.01f ? 0 : AnimationBlend);
        Animator.SetFloat("MotionSpeed", input.normalized.magnitude);


        velocity.y += Physics.gravity.y * Time.deltaTime;
        CCon.Move(velocity * Time.deltaTime);
        if (new Vector3(playerDirection.x, 0, playerDirection.z).magnitude > 0.1f)
        {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z)), Time.deltaTime * 10);
        }

        if (this.index == GameManagerBase.Instance.itIndex && GameManagerBase.Instance.GameState == 1)
        {
            ItEffect.SetInt("Rate", 20);
        }
        else
        {
            ItEffect.SetInt("Rate", 0);
        }
    }


    protected virtual void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var i = Random.Range(0, footstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(footstepAudioClips[i], transform.TransformPoint(CCon.center), FOOTSTEP_AUDIO_VOLUME);
        }
    }

    protected virtual void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.2f && Time.time - OnLandTime > 0.1f)
        {
            OnLandTime = Time.time;
            AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(CCon.center), FOOTSTEP_AUDIO_VOLUME);
        }
    }
}
