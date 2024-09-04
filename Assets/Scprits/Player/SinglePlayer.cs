using UnityEngine;

public class SinglePlayer : PlayerBase
{
    [SerializeField]
    private GameObject circleImage;
    [SerializeField]
    private CapsuleCollider capsuleCollider;
    public int index = -1;

    private float concentrationLevel = 1f;
    private float defaultImageSize = 1f;

    protected override void Awake()
    {
        base.Start();
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

        concentrationLevel = SensorManager.instance.value;

        capsuleCollider.radius = 1.0f + concentrationLevel * 0.5f;
        circleImage.transform.localScale = Vector3.one * defaultImageSize * (1.0f + concentrationLevel * 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: 子オブジェクトの判定も取ってしまうので、修正が必要
        if (other.CompareTag("NPCTrigger"))
        {
            NPCGameManager.instance.ChangeIt(index);
        }
    }
}
