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

        /// <summary>
        /// 次の試行の目標状態（目標提示フェーズで表示用）
        /// </summary>
        private ControlState _nextTargetState;

        public void SetState(ControlState state)
        {
            _text.text = GetJapaneseStateName(state);
            // 目標提示フェーズでは次の目標に応じて色を変更
            var colorState = state == ControlState.GoalPresentation ? _nextTargetState : state;
            _text.color = colorState == ControlState.Excited ? Color.red : Color.white;
        }

        /// <summary>
        /// 次の試行の目標状態を設定（目標提示フェーズで使用）
        /// </summary>
        public void SetNextTargetState(ControlState nextTarget)
        {
            _nextTargetState = nextTarget;
        }

        private string GetJapaneseStateName(ControlState state)
        {
            return state switch
            {
                ControlState.Calibration => "キャリブレーション中",
                ControlState.GoalPresentation => GetGoalPresentationText(),
                ControlState.Preparation => "準備中",
                ControlState.Excited => "目標: 興奮",
                ControlState.Calmed => "目標: 冷静",
                ControlState.Feedback => "結果表示中",
                ControlState.Rest => "休憩中",
                _ => ""
            };
        }

        /// <summary>
        /// 目標提示フェーズ用のテキストを取得（次の目標を含む）
        /// </summary>
        private string GetGoalPresentationText()
        {
            var targetText = _nextTargetState == ControlState.Excited ? "興奮" : "冷静";
            return $"次の目標: {targetText}";
        }
        
        private void Awake()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
        }
    }
}
