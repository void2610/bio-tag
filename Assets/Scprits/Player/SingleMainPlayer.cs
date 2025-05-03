using UnityEngine;
using UnityEngine.VFX;

public class SingleMainPlayer : PlayerBase
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

    private void OnTriggerEnter(Collider other)
    {
        if (GameManagerBase.Instance.GameState != 1)
        {
            return;
        }

        GameManagerBase.Instance.ChangeIt(index);
    }
}
