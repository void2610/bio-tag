using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ControlTask
{
    public enum ControlState
    {
        Excited,
        Calmed
    }
    public class ControlTaskManager : MonoBehaviour
    {
        public static ControlTaskManager Instance;
        
        [SerializeField] private GsrGraph gsrGraph;
        
        public readonly ReactiveProperty<ControlState> TargetState = new();
        public readonly ReactiveProperty<int> Score = new(0);
        public readonly ReactiveProperty<float> CurrentTime = new(0);
        public const float TIME_LIMIT = 60.0f;

        private bool _isGameEnd = false;

        private async UniTaskVoid UpdateTarget()
        {
            TargetState.Value = ControlState.Calmed;
            await UniTask.Delay(15 * 1000);
            TargetState.Value = ControlState.Excited;
            await UniTask.Delay(15 * 1000);
            TargetState.Value = ControlState.Calmed;
            await UniTask.Delay(15 * 1000);
            TargetState.Value = ControlState.Excited;
        }
    
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
            
            UpdateTarget().Forget();
        }

        private void Update()
        {
            if (gsrGraph.IsExcited == (TargetState.Value == ControlState.Excited))
            {
                Score.Value += 1;
            }
            
            CurrentTime.Value += Time.deltaTime;
            
            if (CurrentTime.Value >= TIME_LIMIT && !_isGameEnd)
            {
                _isGameEnd = true;
                Debug.Log($"Score: {Score.Value}");
            }
        }
    }
}
