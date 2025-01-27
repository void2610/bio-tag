using R3;
using TMPro;
using UnityEngine;

namespace GSRGame
{
    public class TimerUI : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private void Start()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
            GsrGameManager.Instance.CurrentTime.Subscribe((t) =>
            {
                _text.text = (GsrGameManager.TIME_LIMIT - t).ToString("F2");
            }).AddTo(this);
        }
    }
}
