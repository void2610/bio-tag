using TMPro;
using UnityEngine;

namespace ControlTask
{
    /// <summary>
    /// タイマーUI - 単純なUIコンポーネント
    /// </summary>
    public class TimerUI : MonoBehaviour
    {
        private TextMeshProUGUI _text;

        public void SetRemainingTime(float remainingTime)
        {
            _text.text = remainingTime.ToString("F1");
        }
        
        private void Awake()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
        }
    }
}
