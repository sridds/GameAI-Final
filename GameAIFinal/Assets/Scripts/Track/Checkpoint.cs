using System.Linq;
using UnityEngine;

// Forward direction is positive z
// It determines the ahead / behindness

namespace Kart.Track
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private float rewardMultiplier = 1f;

        public Collider Col;

        private readonly Color gizmoSelectedArrowColor = Color.red;
        private readonly Color gizmoUnselectedArrowColor = new Color(1.0f, 0.4f, 1.0f, 0.8f);
        private readonly Color gizmoUnselectedColliderColor = new Color(0.8f, 1.0f, 1.0f, 0.8f);
        private readonly Color gizmoFirstColliderColor = new Color(0.2f, 0.8f, 1.0f, 0.8f);
        private readonly float gizmoArrowLength = 3f;
        private readonly float gizmoArrowheadSize = 1.5f;

        public bool IsAheadOf(Vector3 position)
        {
            Vector3 offsetFromCp = position - Col.bounds.center;
            return Vector3.Dot(offsetFromCp, transform.forward) < 0;
        }

        private void OnEnable()
        {
            Col = GetComponent<Collider>();
            Col.isTrigger = true;
        }

        public float GetRewardMultiplier()
        {
            return rewardMultiplier;
        }

        // AI assisted with gizmo code boilerplate
        void OnDrawGizmosSelected()
        {
            // Draw all checkpoints in the track
            LinearTrack track = GetComponentInParent<LinearTrack>();
            if (track != null)
            {
                Checkpoint[] allCheckpoints = track.GetComponentsInChildren<Checkpoint>();
                foreach (Checkpoint cp in allCheckpoints)
                {
                    if (cp.Col == null) continue;

                    bool selected = (cp == this);
                    bool first = (cp == allCheckpoints.First());
                    DrawCheckpointGizmo(cp.Col, cp.transform.forward, selected, first);
                }
            }
            else
            {
                // Fallback if not in a track
                DrawCheckpointGizmo(Col, transform.forward, true);
            }
        }
    
        private void DrawCheckpointGizmo(Collider col, Vector3 forward, bool selected, bool first = false)
        {
            if (first)
            {
                Gizmos.color = gizmoFirstColliderColor;
                DrawColliderGizmo(col);
            } else if (!selected)
            {
                Gizmos.color = gizmoUnselectedColliderColor;
                DrawColliderGizmo(col);
            } 
        
            Gizmos.color = selected ? gizmoSelectedArrowColor : gizmoUnselectedArrowColor;

            // Directional arrow
            Vector3 startPoint = col.bounds.center;
            Vector3 endPoint = col.bounds.center + forward * gizmoArrowLength;
            Gizmos.DrawRay(startPoint, forward * gizmoArrowLength);

            Vector3 rightArrowhead = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 180 + 30, 0) * Vector3.forward;
            Vector3 leftArrowhead = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 180 - 30, 0) * Vector3.forward;

            Gizmos.DrawLine(endPoint, endPoint + rightArrowhead * gizmoArrowheadSize);
            Gizmos.DrawLine(endPoint, endPoint + leftArrowhead * gizmoArrowheadSize);
        }

        private void DrawColliderGizmo(Collider col)
        {
            if (col is BoxCollider boxCol)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(boxCol.transform.position, boxCol.transform.rotation, boxCol.transform.lossyScale);
                Gizmos.DrawWireCube(boxCol.center, boxCol.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawWireSphere(sphereCol.bounds.center, sphereCol.radius * sphereCol.transform.lossyScale.x);
            }
            else if (col is CapsuleCollider capsuleCol)
            {
                Gizmos.DrawWireCube(capsuleCol.bounds.center, capsuleCol.bounds.size);
            }
            else if (col is MeshCollider)
            {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
            else
            {
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
    }
}