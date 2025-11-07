using TMPro;
using UnityEngine;

namespace ControlTask
{
    /// <summary>
    /// スコアUI - 単純なUIコンポーネント
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        
        public void SetScore(int score)
        {
            _text.text = score.ToString();
        }
        
        private void Awake()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
        }
    }
}
