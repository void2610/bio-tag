using UnityEngine;
using UnityEngine.VFX;

public class SingleMainPlayer : PlayerBase
{
    private void Update()
    {
        if (!MyPlayerCamera)
        {
            MyPlayerCamera = transform.GetComponentInChildren<PlayerCamera>().gameObject;
            MyPlayerCamera.name = "PlayerCamera" + index;
        }
        LocalMoving();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Gm?.GameState != 1) return;
        Gm?.ChangeIt(index);
    }
}
