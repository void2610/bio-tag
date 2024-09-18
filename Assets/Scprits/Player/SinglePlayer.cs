using UnityEngine;

public class SinglePlayer : PlayerBase
{
    [SerializeField]
    private GameObject circleImage;
    [SerializeField]
    private CapsuleCollider capsuleCollider;
    public int index = -1;
    private float defaultImageSize = 1f;

    protected override void Awake()
    {
        base.Awake();
        defaultImageSize = circleImage.transform.localScale.x;
    }

    protected override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Instantiate(playerCameraPrefab);
            playerCamera.name = "PlayerCamera";
            playerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
        }
        LocalMoving();

        float r = SensorManager.instance.value ? 5.0f : 0.0f;
        circleImage.transform.localScale = Vector3.one * defaultImageSize * (1.0f + r * 0.5f);
        capsuleCollider.radius = 1.0f + r * 0.5f;
    }

    private void OnTriggerEnter(Collider other)
    {
        NPCGameManager.instance.ChangeIt(index);
    }
}
