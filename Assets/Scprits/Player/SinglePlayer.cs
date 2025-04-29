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
        GameManagerBase.Instance.SetMainPlayer(this);
    }

    protected override void Update()
    {
        base.Update();
        if (!PlayerCamera)
        {
            PlayerCamera = Instantiate(playerCameraPrefab);
            PlayerCamera.name = "PlayerCamera";
            PlayerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
        }
        LocalMoving();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManagerBase.Instance.GameState != 1)
        {
            return;
        }

        GameManagerBase.Instance.ChangeIt(index);
    }
}
