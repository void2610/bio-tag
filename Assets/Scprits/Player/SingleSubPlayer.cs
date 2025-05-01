using UnityEngine;
using UnityEngine.VFX;

public class SingleSubPlayer : PlayerBase
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
        if (!PlayerCamera) CreatePlayerCamera();
        LocalMoving();
    }
    
    private void CreatePlayerCamera()
    {
        if (PlayerCamera) return;
        
        PlayerCamera = Instantiate(playerCameraPrefab);
        PlayerCamera.name = "PlayerCamera";
        Destroy(PlayerCamera.GetComponent<AudioListener>());
        PlayerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
        PlayerCamera.GetComponent<Camera>().targetDisplay = index;
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
