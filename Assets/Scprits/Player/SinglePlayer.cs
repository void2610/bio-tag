using UnityEngine;
using UnityEngine.VFX;

public class SinglePlayer : PlayerBase
{
    public int index = -1;
    private VisualEffect itEffect;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        itEffect = transform.Find("ItEffect").GetComponent<VisualEffect>();
    }

    protected override void Update()
    {
        base.Update();
        if (playerCamera == null)
        {
            playerCamera = Instantiate(playerCameraPrefab);
            playerCamera.name = "PlayerCamera";
            playerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
        }
        LocalMoving();

        if (index == GameManagerBase.instance.itIndex && GameManagerBase.instance.gameState == 1)
        {
            itEffect.SetInt("Rate", 20);
        }
        else
        {
            itEffect.SetInt("Rate", 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManagerBase.instance.gameState != 1)
        {
            return;
        }

        NPCGameManager.instance.ChangeIt(index);
    }
}
