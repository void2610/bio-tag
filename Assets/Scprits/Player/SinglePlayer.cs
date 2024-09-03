using UnityEngine;

public class SinglePlayer : PlayerBase
{
    public int index = -1;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            NPCGameManager.instance.ChangeIt(index);
        }
    }
}
