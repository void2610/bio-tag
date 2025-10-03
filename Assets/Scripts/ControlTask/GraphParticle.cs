using UnityEngine;
using UnityEngine.VFX;

namespace ControlTask
{
    public class GraphParticle : MonoBehaviour
    {
        [SerializeField] private GsrGraph graph;
        private VisualEffect _graphParticle;
        private bool _isPlaying = false;

        private void Awake()
        {
            _graphParticle = this.GetComponent<VisualEffect>();
            _graphParticle.Stop();
        }

        private void Start()
        {
            graph.SetLineColor(Color.white);
        }

        private void Update()
        {
            var target = ControlTaskManager.Instance.TargetState.Value == ControlState.Excited;
            if (graph.IsExcited != target && !_isPlaying)
            {
                _isPlaying = true;
                _graphParticle.Play();
                graph.SetLineColor(Color.red);
                CameraMove.Instance.StartShake(0.25f);
            }
            else if (!graph.IsExcited != target && _isPlaying)
            {
                _isPlaying = false;
                _graphParticle.Stop();
                graph.SetLineColor(Color.white);
                CameraMove.Instance.StopShake();
            }
            
            this.transform.position = graph.GetLastData();
        }
    
    }
}
