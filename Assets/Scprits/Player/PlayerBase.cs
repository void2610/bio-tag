using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem;
using VContainer;

public class PlayerBase : MonoBehaviour
{
    [SerializeField] protected GameObject playerCameraPrefab;
    [SerializeField] protected AudioClip[] footstepAudioClips;
    [SerializeField] protected AudioClip landingAudioClip;
    [SerializeField] protected float jumpPower = 6f;
    [SerializeField] protected float walkSpeed = 6f;
    public Vector3 velocity = Vector3.zero;
    public int Index { get; private set; } = -1;

    protected Animator Animator => GetComponent<Animator>();
    protected CharacterController CCon => GetComponent<CharacterController>();
    protected GameObject MyPlayerCamera = null;
    protected float AnimationBlend = 0f;
    protected Vector3 LookDirection = Vector3.zero;
    protected float OnLandTime = 0f;
    protected bool IsMovable = true;
    protected VisualEffect ItEffect;
    protected Vector3 MovementInput;
    protected bool JumpInput = false;
    protected IGameManagerService Gm;
    private const float FOOTSTEP_AUDIO_VOLUME = 0.5f;

    public void SetWalkSpeed(float s) => walkSpeed = s;

    public void Initialize(IGameManagerService gameManager, int index)
    {
        Index = index;
        Gm = gameManager;
        ItEffect = transform.Find("ItEffect").GetComponent<VisualEffect>();
    }

    public void OnMove(InputValue value)
    {
        MovementInput = new Vector3(value.Get<Vector2>().x, 0, value.Get<Vector2>().y);
    }
    
    public void OnJump(InputValue value)
    {
        JumpInput = value.isPressed;
    }
    
    protected void LocalMoving()
    {
        UpdateCharacterController(MovementInput, MyPlayerCamera.transform.forward, JumpInput);
        LookDirection = MyPlayerCamera.transform.forward;
        JumpInput = false;
    }

    protected void UpdateCharacterController(Vector3 input, Vector3 playerDirection, bool isJump)
    {
        if (!IsMovable)
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

        // VContainerからゲーム状態を取得してItエフェクトを制御
        var isGamePlaying = Gm?.GameState == 1;
        var isIt = this.Index == Gm?.ItIndex;
        
        if (isIt && isGamePlaying)
        {
            ItEffect?.SetInt("Rate", 20);
        }
        else
        {
            ItEffect?.SetInt("Rate", 0);
        }
    }
    
    private void Update()
    {
        if (!MyPlayerCamera)
        {
            MyPlayerCamera = transform.GetComponentInChildren<PlayerCamera>().gameObject;
            MyPlayerCamera.name = "PlayerCamera" + Index;
        }
        LocalMoving();
    }

    protected void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            var i = Random.Range(0, footstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(footstepAudioClips[i], transform.TransformPoint(CCon.center), FOOTSTEP_AUDIO_VOLUME);
        }
    }

    protected void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.2f && Time.time - OnLandTime > 0.1f)
        {
            OnLandTime = Time.time;
            AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(CCon.center), FOOTSTEP_AUDIO_VOLUME);
        }
    }
}
