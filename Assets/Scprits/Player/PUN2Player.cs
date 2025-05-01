using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using Photon.Pun;

public class PUN2Player : PlayerBase, IPunObservable
{
    protected override void Start()
    {
        base.Start();

        if (PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            // GameManagerにプレイヤーを登録
            GameManagerBase.Instance.SetMainPlayer(this);
        }
        if (!photonView.IsMine)
        {
            // VFXをセンサーマネージャーに登録
            var vfx = transform.Find("PlayerTrail").GetComponent<VisualEffect>();
            SensorManager.Instance.AddVFX(vfx);
        }
    }

    protected override void Update()
    {
        if (photonView.IsMine)
        {
            if (!PlayerCamera)
            {
                PlayerCamera = Instantiate(playerCameraPrefab);
                PlayerCamera.name = "PlayerCamera";
                PlayerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
            }
        }
    }

    // 位置と回転の同期
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (photonView.IsMine && other.CompareTag("Player") && PhotonNetwork.CurrentRoom.TryGetLastTagTime(out var lastTagTime))
        {
            if (PhotonNetwork.ServerTimestamp - lastTagTime < 1000 || !PhotonNetwork.CurrentRoom.TryGetItIndex(out var itIndex))
            {
                return;
            }

            var targetPlayer = other.gameObject.GetComponent<PhotonView>();
            if (itIndex == photonView.OwnerActorNr)
            {
                PhotonNetwork.CurrentRoom.SetItIndex(targetPlayer.OwnerActorNr, PhotonNetwork.ServerTimestamp);
            }
            else if (itIndex == targetPlayer.OwnerActorNr)
            {
                PhotonNetwork.CurrentRoom.SetItIndex(photonView.OwnerActorNr, PhotonNetwork.ServerTimestamp);
            }
        }
    }


    protected override void OnFootstep(AnimationEvent animationEvent)
    {
        if (!photonView.IsMine) return;
        base.OnFootstep(animationEvent);
    }

    protected override void OnLand(AnimationEvent animationEvent)
    {
        if (!photonView.IsMine) return;
        base.OnLand(animationEvent);
    }
}
