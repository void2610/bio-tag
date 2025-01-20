using R3;
using UnityEngine;

namespace GSRGame
{
    public class GsrGameManager : MonoBehaviour
    {
        public static GsrGameManager Instance;
        public readonly ReactiveProperty<int> Score = new(0);
    
        public void AddScore(int score) => Score.Value += score;
        public void ResetScore() => Score.Value = 0;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Update()
        {
            if (!GsrGraph.Instance.IsExcited)
            {
                AddScore(1);
            }
        }
    }
}
