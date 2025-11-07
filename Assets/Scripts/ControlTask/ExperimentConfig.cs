using UnityEngine;

namespace ControlTask
{
    /// <summary>
    /// 実験の設定パラメータ
    /// </summary>
    [System.Serializable]
    public class ExperimentConfig
    {
        [Header("キャリブレーション")]
        public float calibrationDuration = 30f;

        [Header("試行設定")]
        public int trialCount = 9;
        public float goalPresentationDuration = 3f;
        public float preparationDuration = 2f;
        public float measurementDuration = 20f;
        public float feedbackDuration = 5f;
        public float restDuration = 0f;
    }
}
