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
            ControlTaskManager.Instance.CurrentTime.Subscribe((t) =>
            {
                _text.text = (ControlTaskManager.Instance.TotalDuration - t).ToString("F2");
            }).AddTo(this);
        }
    }
}
