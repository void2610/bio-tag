using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace GSRGame
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

        private void Update()
        {
            if (graph.IsExcited && !_isPlaying)
            {
                _isPlaying = true;
                _graphParticle.Play();
                CameraMove.Instance.StartShake(0.25f);
            }
            else if (!graph.IsExcited && _isPlaying)
            {
                _isPlaying = false;
                _graphParticle.Stop();
                CameraMove.Instance.StopShake();
            }
            
            this.transform.position = graph.GetLastData();
        }
    
    }
}
