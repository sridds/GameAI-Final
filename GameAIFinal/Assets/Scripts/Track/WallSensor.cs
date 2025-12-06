using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kart.Track
{
    // Pattern ensures that you're not getting multiple penalties for hitting two walls at once
    // Take the maximum penalty per tick of all the ones you're bumping into
    public class WallSensor : MonoBehaviour
    {
        public UnityEvent<float> onApplyPenalty;
        
        private readonly List<Wall> collidingWalls = new List<Wall>();
        private float maxInitialPenalty;
        private float maxContinuousPenalty;

        // Not sure if Update would screw with ML training process, so stuck to FixedUpdate
        private void FixedUpdate()
        {
            if (maxContinuousPenalty == 0)
                return;
            
            onApplyPenalty?.Invoke(maxContinuousPenalty * Time.fixedDeltaTime);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Wall"))
                return;

            if (!other.gameObject.TryGetComponent<Wall>(out Wall wall))
                return;
            
            collidingWalls.Add(wall);
            float lastInitialPenalty = maxInitialPenalty;
            UpdateMaxPenalties();
            if (maxContinuousPenalty > lastInitialPenalty)
            {
                float penaltyDiff = maxInitialPenalty - lastInitialPenalty;
                onApplyPenalty?.Invoke(penaltyDiff);
            }
        }
        
        private void OnCollisionExit(Collision other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Wall"))
                return;

            if (!other.gameObject.TryGetComponent<Wall>(out Wall wall))
                return;
            
            collidingWalls.Remove(wall);
            UpdateMaxPenalties();
        }

        private void UpdateMaxPenalties()
        {
            maxInitialPenalty = 0;
            maxContinuousPenalty = 0;
            
            foreach (Wall wall in collidingWalls)
            {
                if (wall.penaltyPerSecondMultiplier > maxContinuousPenalty)
                    maxContinuousPenalty = wall.penaltyPerSecondMultiplier;

                if (wall.initialPenaltyMultiplier > maxInitialPenalty)
                    maxInitialPenalty = wall.initialPenaltyMultiplier;
            }
        }
    }
}
