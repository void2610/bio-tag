using R3;
using TMPro;
using UnityEngine;

namespace ControlTask
{
    public class ScoreUI : MonoBehaviour
    {
        private void Start()
        {
            ControlTaskManager.Instance.Score.Subscribe(s =>
            {
                this.GetComponent<TextMeshProUGUI>().text = s.ToString();
            }).AddTo(this);
        }
    }
}
