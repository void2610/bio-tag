using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace ControlTask
{
    public enum ControlState
    {
        Excited,
        Calmed,
        Rest
    }
    public class ControlTaskManager : MonoBehaviour
    {
        public static ControlTaskManager Instance;

        [SerializeField] private GsrGraph gsrGraph;

        [Header("Task Settings")]
        [SerializeField] private float calmedDuration = 15f;
        [SerializeField] private float excitedDuration = 15f;
        [SerializeField] private float restDuration = 5f;
        [SerializeField] private int repeatCount = 2;

        public readonly ReactiveProperty<ControlState> TargetState = new();
        public readonly ReactiveProperty<int> Score = new(0);
        public readonly ReactiveProperty<float> CurrentTime = new(0);
        
        public float TotalDuration => (calmedDuration + excitedDuration + 2 * restDuration) * repeatCount;

        // 固定パターン: Calmed → Rest → Excited → Rest を繰り返す
        private async UniTaskVoid UpdateTarget()
        {
            for (var i = 0; i < repeatCount; i++)
            {
                // Calmed状態
                TargetState.Value = ControlState.Calmed;
                await UniTask.Delay((int)(calmedDuration * 1000));

                // 休憩
                TargetState.Value = ControlState.Rest;
                await UniTask.Delay((int)(restDuration * 1000));

                // Excited状態
                TargetState.Value = ControlState.Excited;
                await UniTask.Delay((int)(excitedDuration * 1000));

                // 休憩
                TargetState.Value = ControlState.Rest;
                await UniTask.Delay((int)(restDuration * 1000));
            }

            // 全パターン完了後、タスク終了
            Debug.Log($"Task Complete! Score: {Score.Value}");
        }
    
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
            
            UpdateTarget().Forget();
        }

        private void Update()
        {
            // Rest状態ではスコアをカウントしない
            if (TargetState.Value != ControlState.Rest)
            {
                if (gsrGraph.IsExcited == (TargetState.Value == ControlState.Excited))
                {
                    Score.Value += 1;
                }
            }

            // 経過時間を記録
            CurrentTime.Value += Time.deltaTime;
        }
    }
}
