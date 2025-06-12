using UnityEngine;
using UnityEngine.VFX;

public class SingleSubPlayer : PlayerBase
{
    private void CreatePlayerCamera()
    {
        if (MyPlayerCamera) return;
        
        MyPlayerCamera = Instantiate(playerCameraPrefab, this.transform);
        MyPlayerCamera.name = "PlayerCamera";
        Destroy(MyPlayerCamera.GetComponent<AudioListener>());
        MyPlayerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
        MyPlayerCamera.GetComponent<Camera>().targetDisplay = Index;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Gm?.GameState != 1)
        {
            return;
        }

        Gm?.ChangeIt(Index);
    }
}
