using UnityEngine;
using TMPro;
using Photon.Pun;

public partial class TimerUI : MonoBehaviour
{
    private TMP_Text _timerText;

    private void Start()
    {
        _timerText = this.GetComponent<TMP_Text>();
        _timerText.text = "";
    }

    private void Update()
    {
        if (GameManagerBase.Instance.GetType() == typeof(NetworkGameManager))
        {
            if (!PhotonNetwork.InRoom) { return; }
            if (GameManagerBase.Instance.GameState != 1) { return; }
            if (!PhotonNetwork.CurrentRoom.TryGetStartTime(out var timestamp)) { return; }

            var elapsedTime = Mathf.Max(0f, unchecked(PhotonNetwork.ServerTimestamp - timestamp) / 1000f);
            _timerText.text = elapsedTime.ToString("f2");
        }
        else if (GameManagerBase.Instance.GetType() == typeof(NpcGameManager))
        {
            var i = (NpcGameManager)GameManagerBase.Instance;
            if (i.GameState != 1) return;
            var elapsedTime = i.getElapsedTime();
            _timerText.text = elapsedTime.ToString("f2");
        }
    }
}
