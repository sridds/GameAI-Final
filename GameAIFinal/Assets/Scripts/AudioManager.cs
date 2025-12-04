using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Kart
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        [Header("References")]
        [SerializeField]
        private AudioObject _soundPrefab;

        private ObjectPool<AudioObject> pool;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                pool = new ObjectPool<AudioObject>(CreateSource, OnTakeFromPool, OnReturnToPool);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Creates new pooled AudioSource
        /// </summary>
        /// <param name="clip"></param>
        public void PlayAudio(SoundStreamSO clip, float delay = 0.0f)
        {
            if (!CanPlay(clip)) return;

            if (delay > 0.0f) StartCoroutine(PlayAudioDelayed(clip, delay));
            else CreateAudio(clip);
        }

        /// <summary>
        /// Creates new pooled AudioSource at point
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="point"></param>
        public void PlayAudioAtPoint(SoundStreamSO clip, Vector3 point, float delay = 0.0f)
        {
            if (!CanPlay(clip)) return;

            if (!clip.doSpatialAudio)
            {
                Debug.LogWarning("Failed to play Audio at point! Clip does not have spatial audio enabled.");
                return;
            }

            if (delay > 0.0f) StartCoroutine(PlayAudioAtPointDelayed(clip, point, delay));
            else CreateAudioAtPoint(clip, point);
        }

        public void PlayAudioTracked(SoundStreamSO clip, Transform trackedTransform, float delay = 0.0f)
        {
            if (!CanPlay(clip)) return;

            if (delay > 0.0f) StartCoroutine(PlayAudioTrackedDelayed(clip, trackedTransform, delay));
            else CreateAudioTracked(clip, trackedTransform);
        }

        #region Helpers
        private bool CanPlay(SoundStreamSO clip)
        {
            if (clip == null)
            {
                Debug.LogWarning($"Tried to play a null SoundStream");
                return false;
            }

            if (clip.GetClip() == null)
            {
                Debug.LogWarning($"Cannot play {clip.name} with a null clip!");
                return false;
            }

            return true;
        }

        private void CreateAudio(SoundStreamSO clip)
        {
            // Create and play object (2D)
            AudioObject audioObj = SpawnAudioObject(clip);
            audioObj.source.spatialBlend = 0;
            audioObj.source.Play();
        }

        private void CreateAudioAtPoint(SoundStreamSO clip, Vector3 point)
        {
            // Create and play object at point
            AudioObject audioObj = SpawnAudioObject(clip);
            audioObj.transform.position = point;

            if (clip.doSpatialAudio)
            {
                audioObj.source.spatialBlend = 1;
                audioObj.source.minDistance = clip.minDistance;
                audioObj.source.maxDistance = clip.maxDistance;
            }

            audioObj.source.Play();
        }

        private void CreateAudioTracked(SoundStreamSO clip, Transform trackedTransform)
        {
            // Create and play object at point
            AudioObject audioObj = SpawnAudioObject(clip);

            // sets point and parent
            audioObj.transform.position = trackedTransform.position;
            audioObj.SetTrackedObject(trackedTransform);

            if (clip.doSpatialAudio)
            {
                audioObj.source.spatialBlend = 1;
                audioObj.source.minDistance = clip.minDistance;
                audioObj.source.maxDistance = clip.maxDistance;
            }

            audioObj.source.Play();
        }

        private IEnumerator PlayAudioDelayed(SoundStreamSO clip, float delay)
        {
            yield return new WaitForSeconds(delay);

            CreateAudio(clip);
        }

        private IEnumerator PlayAudioAtPointDelayed(SoundStreamSO clip, Vector3 point, float delay)
        {
            yield return new WaitForSeconds(delay);

            CreateAudioAtPoint(clip, point);
        }

        private IEnumerator PlayAudioTrackedDelayed(SoundStreamSO clip, Transform trackedTransform, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (trackedTransform != null) CreateAudioTracked(clip, trackedTransform);
        }

        private AudioObject SpawnAudioObject(SoundStreamSO clip)
        {
            pool.Get(out AudioObject audioObj);

            audioObj.source.clip = clip.GetClip();
            audioObj.source.pitch = clip.GetPitch();
            audioObj.source.volume = clip.volume;

            return audioObj;
        }

        private AudioObject CreateSource()
        {
            var source = Instantiate(_soundPrefab);
            source.transform.parent = transform;

            return source;
        }

        private void OnTakeFromPool(AudioObject audioObj)
        {
            audioObj.gameObject.SetActive(true);
            audioObj.Initialize(pool);
        }

        private void OnReturnToPool(AudioObject audioObj)
        {
            audioObj.gameObject.SetActive(false);
            audioObj.source.Stop();
        }
        #endregion
    }
}
