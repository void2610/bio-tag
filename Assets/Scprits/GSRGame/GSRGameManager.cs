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
        public readonly ReactiveProperty<GsrState> TargetState = new(GsrState.Calmed);
        public readonly ReactiveProperty<int> Score = new(0);
        public readonly ReactiveProperty<float> Time = new(0);
        public const float TIME_LIMIT = 30.0f;

        private async UniTaskVoid UpdateTarget()
        {
            TargetState.Value = GsrState.Calmed;
            await UniTask.Delay((int)(Random.Range(10.0f, 15.0f) * 1000));
            TargetState.Value = GsrState.Excited;
            await UniTask.Delay((int)(Random.Range(10.0f, 15.0f) * 1000));
            TargetState.Value = GsrState.Calmed;
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
            
            Time.Value += UnityEngine.Time.deltaTime;
        }
    }
}
