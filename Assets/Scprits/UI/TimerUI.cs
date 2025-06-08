using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    private TMP_Text _timerText;

    private void Start()
    {
        _timerText = this.GetComponent<TMP_Text>();
        _timerText.text = "";
    }

    private void Update()
    {
        if (GameManagerBase.Instance.GetType() == typeof(NpcGameManager))
        {
            var i = (NpcGameManager)GameManagerBase.Instance;
            if (i.GameState != 1) return;
            var elapsedTime = i.GetElapsedTime();
            _timerText.text = elapsedTime.ToString("f2");
        }
    }
}
