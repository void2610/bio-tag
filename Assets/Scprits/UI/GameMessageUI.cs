using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameMessageUI : MonoBehaviour
{
    private TMP_Text messageText;
    public void SetMessage(string message)
    {
        messageText.text = message;
    }
    void Start()
    {
        messageText = this.GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (!PhotonNetwork.InRoom) { return; }
        PhotonNetwork.CurrentRoom.TryGetGameState(out int gameState);
        if (gameState == 1)
        {
            messageText.text = "";
        }
    }
}
