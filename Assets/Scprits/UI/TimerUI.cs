using UnityEngine;
using TMPro;
using Photon.Pun;

public class TimerUI : MonoBehaviour
{
    private TMP_Text timerText;
    void Start()
    {
        timerText = this.GetComponent<TMP_Text>();
        timerText.text = "0.00";
    }

    private void Update()
    {
        if (!PhotonNetwork.InRoom) { return; }
        if (!PhotonNetwork.CurrentRoom.TryGetStartTime(out int timestamp)) { return; }

        float elapsedTime = Mathf.Max(0f, unchecked(PhotonNetwork.ServerTimestamp - timestamp) / 1000f);
        timerText.text = elapsedTime.ToString("f2");
    }
}
