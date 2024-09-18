using UnityEngine;

public class SinglePlayer : PlayerBase
{
    [SerializeField]
    private GameObject circleImage;
    public int index = -1;

    private float concentrationLevel = 1f;
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

        concentrationLevel = SensorManager.instance.value;

        circleImage.transform.localScale = Vector3.one * defaultImageSize * (1.0f + concentrationLevel * 0.5f);
    }
}
