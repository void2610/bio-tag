using UnityEngine;

namespace BioTag.Audio
{
    /// <summary>
    /// AudioService用の設定ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "BioTag/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("足音設定")]
        [Tooltip("足音AudioClip配列")]
        public AudioClip[] footstepClips;

        [Header("着地音設定")]
        [Tooltip("着地音AudioClip")]
        public AudioClip landingClip;

        [Header("音量設定")]
        [Range(0f, 1f)]
        [Tooltip("足音の再生音量 (0.0-1.0)")]
        public float footstepVolume = 0.5f;

        [Range(0f, 1f)]
        [Tooltip("着地音の再生音量 (0.0-1.0)")]
        public float landingVolume = 0.5f;
    }
}
