using UnityEngine;
using TMPro;
using Photon.Pun;

public class TimerUI : MonoBehaviour
{
    private TMP_Text timerText;
    void Start()
    {
        timerText = this.GetComponent<TMP_Text>();
        timerText.text = "";
    }

    private void Update()
    {
        if (GameManagerBase.instance.GetType() == typeof(NetworkGameManager))
        {
            if (!PhotonNetwork.InRoom) { return; }
            if (GameManagerBase.instance.gameState != 1) { return; }
            if (!PhotonNetwork.CurrentRoom.TryGetStartTime(out int timestamp)) { return; }

            float elapsedTime = Mathf.Max(0f, unchecked(PhotonNetwork.ServerTimestamp - timestamp) / 1000f);
            timerText.text = elapsedTime.ToString("f2");
        }
        else if (GameManagerBase.instance.GetType() == typeof(NPCGameManager))
        {
            NPCGameManager i = (NPCGameManager)GameManagerBase.instance;
            if (i.gameState != 1) return;
            float elapsedTime = i.getElapsedTime();
            timerText.text = elapsedTime.ToString("f2");
        }

    }
}
