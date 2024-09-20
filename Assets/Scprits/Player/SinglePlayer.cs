using UnityEngine;
using UnityEngine.VFX;

public class SinglePlayer : PlayerBase
{
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
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
