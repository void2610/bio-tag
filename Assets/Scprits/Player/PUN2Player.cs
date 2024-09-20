using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using Photon.Pun;

public class PUN2Player : PlayerBase, IPunObservable
{
    protected override void Start()
    {
        base.Start();

        if (!photonView.IsMine)
        {
            // VFXをセンサーマネージャーに登録
            var vfx = transform.Find("PlayerTrail").GetComponent<VisualEffect>();
            SensorManager.instance.AddVFX(vfx);
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (playerCamera == null)
            {
                playerCamera = Instantiate(playerCameraPrefab);
                playerCamera.name = "PlayerCamera";
                playerCamera.GetComponent<PlayerCamera>().target = this.transform.Find("PlayerCameraRoot").gameObject.transform;
            }
            LocalMoving();
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
        if (photonView.IsMine && other.CompareTag("Player") && PhotonNetwork.CurrentRoom.TryGetLastTagTime(out double lastTagTime))
        {
            if (PhotonNetwork.ServerTimestamp - lastTagTime < 1000 || !PhotonNetwork.CurrentRoom.TryGetItIndex(out int itIndex))
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
