using System;
using UnityEngine;

namespace Kart.Track
{
    [RequireComponent(typeof(Collider))]
    public class Wall : MonoBehaviour
    {
        public float initialPenaltyMultiplier = 0.5f;
        public float penaltyPerSecondMultiplier = 0.1f;
        
        private Collider col;
        
        void Awake()
        {
            col = GetComponent<Collider>();
            gameObject.layer = LayerMask.NameToLayer("Wall");
        }
    }
}
