using UnityEngine;
using UnityEngine.Pool;

namespace Kart
{
    public class AudioObject : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _source;

        ObjectPool<AudioObject> myPool;
        private bool initalized;
        private bool inPool;
        private Transform trackedObject;

        public AudioSource source { get { return _source; } }

        public void Initialize(ObjectPool<AudioObject> pool)
        {
            myPool = pool;

            initalized = true;
            inPool = false;
            trackedObject = null;
        }

        public void SetTrackedObject(Transform trackedObject)
        {
            this.trackedObject = trackedObject;
        }

        private void Update()
        {
            if (!initalized) return;

            if (trackedObject != null)
            {
                transform.position = trackedObject.position;
            }

            // Return object to the pool
            if (!_source.isPlaying && !inPool)
            {
                myPool.Release(this);
                inPool = true;
            }
        }
    }

}
