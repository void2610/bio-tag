using TMPro;
using R3;
using UnityEngine;

namespace ControlTask
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TargetStateUI : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private void Start()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
            ControlTaskManager.Instance.TargetState.Subscribe((state) =>
            {
                _text.text = GetJapaneseStateName(state);
                _text.color = state == ControlState.Excited ? Color.red : Color.white;
            }).AddTo(this);
        }
    
        private string GetJapaneseStateName(ControlState state)
        {
            return state switch
            {
                ControlState.Calibration => "キャリブレーション中",
                ControlState.GoalPresentation => "目標表示中",
                ControlState.Preparation => "準備中",
                ControlState.Excited => "目標: 興奮",
                ControlState.Calmed => "目標: 冷静",
                ControlState.Feedback => "結果表示中",
                ControlState.Rest => "休憩中",
                _ => ""
            };
        }
    }
}
