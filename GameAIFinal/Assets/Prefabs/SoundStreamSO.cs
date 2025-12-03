using UnityEngine;

namespace Kart
{
    [CreateAssetMenu(menuName = "Super ML Kart/Sound Stream", fileName = "New Sound Stream")]
    public class SoundStreamSO : ScriptableObject
    {
        [SerializeField]
        private AudioClip[] clip;

        [Header("Pitch")]
        [SerializeField]
        private float pitch;
        [SerializeField]
        private Vector2 randomPitchDeviation;

        [Header("Volume / 3D Sound")]
        public float volume;
        public bool doSpatialAudio;
        public int minDistance;
        public int maxDistance;

        public float GetPitch()
        {
            return pitch + Random.Range(randomPitchDeviation.x, randomPitchDeviation.y);
        }

        public AudioClip GetClip()
        {
            return clip[Random.Range(0, clip.Length)];
        }
    }

}
