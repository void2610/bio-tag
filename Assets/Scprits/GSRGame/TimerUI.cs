using UnityEngine;

namespace GSRGame
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private float timeLimit = 30.0f;
        private float _time = 0;
        
        public void ResetTime() => _time = 0;
        public float GetTime() => _time;
        
        private void Update()
        {
            _time += Time.deltaTime;
            this.GetComponent<TMPro.TextMeshProUGUI>().text = _time.ToString("F2");
            if (_time >= timeLimit)
            {
                GsrGameManager.Instance.ResetScore();
                _time = 0;
            }
        }
    }
}
