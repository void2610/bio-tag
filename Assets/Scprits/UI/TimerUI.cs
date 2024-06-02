using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    private TMP_Text timerText;
    void Start()
    {
        //GameManager.instance.TimerValue.OnValueChanged += OnTimerValueChanged;
        timerText = this.GetComponent<TMP_Text>();
        //timerText.text = GameManager.instance.TimerValue.Value.ToString("F2");
    }

    private void OnTimerValueChanged(float oldValue, float newValue)
    {
        timerText.text = newValue.ToString("F2");
    }

    private void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            //GameManager.instance.TimerValue.OnValueChanged -= OnTimerValueChanged;
        }
    }
}
