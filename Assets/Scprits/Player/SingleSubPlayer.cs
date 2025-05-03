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
        if (!MyPlayerCamera)
        {
            MyPlayerCamera = transform.GetComponentInChildren<PlayerCamera>().gameObject;
            MyPlayerCamera.name = "PlayerCamera" + index;
        }
        LocalMoving();
    }
    
    private void CreatePlayerCamera()
    {
        if (MyPlayerCamera) return;
        
        MyPlayerCamera = Instantiate(playerCameraPrefab, this.transform);
        MyPlayerCamera.name = "PlayerCamera";
        Destroy(MyPlayerCamera.GetComponent<AudioListener>());
        MyPlayerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
        MyPlayerCamera.GetComponent<Camera>().targetDisplay = index;
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
