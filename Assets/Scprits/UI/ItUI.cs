using UnityEngine;
using Photon.Pun;
using TMPro;

public class ItUI : MonoBehaviour
{
    private TMP_Text _itText;
    private void Start()
    {
        _itText = this.GetComponent<TMP_Text>();
        _itText.text = "";
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom) { return; }
        if (!PhotonNetwork.CurrentRoom.TryGetItIndex(out var itIndex)) { return; }

        _itText.text = $"It: {PhotonNetwork.PlayerList[itIndex - 1].NickName}";
    }
}
