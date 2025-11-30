using UnityEngine;
using UnityEngine.VFX;
using VitalRouter;
using BioTag.GameUI;

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
        if (Gm?.GameState != 1) return;

        // プレイヤー同士の接触のみを検出
        if (!other.CompareTag("Player")) return;

        // 鬼でない場合のみタグイベントを発行（鬼にタッチされた時）
        if (Index == Gm?.CurrentItIndex) return;

        // 自分自身のTransformを含めてCommandを発行
        Router.Default.PublishAsync(new PlayerTaggedCommand(Index, transform));
    }
}
