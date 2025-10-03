using TMPro;
using R3;
using UnityEngine;

namespace ControlTask
{
    public class TargetStateUI : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private void Start()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
            ControlTaskManager.Instance.TargetState.Subscribe((state) =>
            {
                _text.text = state.ToString();
                _text.color = state == ControlState.Excited ? Color.red : Color.white;
            }).AddTo(this);
        }
    }
}
