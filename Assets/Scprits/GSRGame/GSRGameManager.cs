using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace GSRGame
{
    public enum GsrState
    {
        Excited,
        Calmed
    }
    public class GsrGameManager : MonoBehaviour
    {
        public static GsrGameManager Instance;
        public readonly ReactiveProperty<GsrState> TargetState = new();
        public readonly ReactiveProperty<int> Score = new(0);
        public readonly ReactiveProperty<float> CurrentTime = new(0);
        public const float TIME_LIMIT = 60.0f;
        
        private bool _isGameEnd = false;

        private async UniTaskVoid UpdateTarget()
        {
            TargetState.Value = GsrState.Calmed;
            await UniTask.Delay(15 * 1000);
            TargetState.Value = GsrState.Excited;
            await UniTask.Delay(15 * 1000);
            TargetState.Value = GsrState.Calmed;
            await UniTask.Delay(15 * 1000);
            TargetState.Value = GsrState.Excited;
        }
    
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
            
            UpdateTarget().Forget();
        }

        private void Update()
        {
            if (GsrGraph.Instance.IsExcited == (TargetState.Value == GsrState.Excited))
            {
                Score.Value += 1;
            }
            
            CurrentTime.Value += UnityEngine.Time.deltaTime;
            
            if (CurrentTime.Value >= TIME_LIMIT && !_isGameEnd)
            {
                _isGameEnd = true;
                Debug.Log($"Score: {Score.Value}");
            }
        }
    }
}
