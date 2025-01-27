using TMPro;
using R3;
using UnityEngine;

namespace GSRGame
{
    public class TargetStateUI : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private void Start()
        {
            _text = this.GetComponent<TextMeshProUGUI>();
            GsrGameManager.Instance.TargetState.Subscribe((state) =>
            {
                _text.text = state.ToString();
                _text.color = state == GsrState.Excited ? Color.red : Color.white;
            }).AddTo(this);
        }
    }
}
