using R3;
using TMPro;
using UnityEngine;

namespace GSRGame
{
    public class ScoreUI : MonoBehaviour
    {
        private void Start()
        {
            GsrGameManager.Instance.Score.Subscribe(s => 
            { 
                this.GetComponent<TextMeshProUGUI>().text = s.ToString();
            }).AddTo(this);
        }
    }
}
