using UnityEngine;
using Photon.Pun;
using TMPro;

public class ItUI : MonoBehaviour
{
    private TMP_Text itText;
    void Start()
    {
        itText = this.GetComponent<TMP_Text>();
        itText.text = "";
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) { return; }
        if (!PhotonNetwork.CurrentRoom.TryGetItIndex(out int itIndex)) { return; }

        itText.text = $"It: {PhotonNetwork.PlayerList[itIndex - 1].NickName}";
    }
}
