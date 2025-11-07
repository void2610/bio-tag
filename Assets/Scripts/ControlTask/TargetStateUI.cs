using TMPro;
using UnityEngine;

namespace ControlTask
{
    /// <summary>
    /// 目標状態表示UI - 単純なUIコンポーネント
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TargetStateUI : MonoBehaviour
    {
        private TextMeshProUGUI _text;

        public void SetState(ControlState state)
        {
            _text.text = GetJapaneseStateName(state);
            _text.color = state == ControlState.Excited ? Color.red : Color.white;
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
        
        private void Awake()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
        }
    }
}
