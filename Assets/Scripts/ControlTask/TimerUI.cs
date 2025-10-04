using R3;
using TMPro;
using UnityEngine;

namespace ControlTask
{
    public class TimerUI : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private void Start()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
            // 現在のフェーズの残り時間を表示
            ControlTaskManager.Instance.CurrentTime.Subscribe(_ =>
            {
                var remainingTime = ControlTaskManager.Instance.CurrentPhaseRemainingTime;
                _text.text = remainingTime.ToString("F1");
            }).AddTo(this);
        }
    }
}
