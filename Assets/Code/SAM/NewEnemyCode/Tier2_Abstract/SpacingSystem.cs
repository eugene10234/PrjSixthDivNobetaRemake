using UnityEngine;
using VContainer.Unity;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Calculates a lateral avoidance vector from nearby enemies and obstacles,
    /// and writes it into BaseEnemyData.AvoidanceDirection each FixedTick.
    /// BGMovementActor reads AvoidanceDirection and blends it into velocity.
    ///
    /// Plain C# ¡X no MonoBehaviour needed. Registered as IFixedTickable so
    /// VContainer ticks it automatically in FixedUpdate.
    /// </summary>
    public class SpacingSystem : IFixedTickable
    {
        private readonly BaseEnemyData _data;
        private readonly IMovementActor _movement;

        private const float Radius = 20f;
        private const float ObstacleWeight = 3.5f;
        private const float TickInterval = 0.1f; // rate-limit the OverlapSphere
        private float _timer;

        private readonly int _enemyMask;
        private readonly int _obstacleMask;
        private readonly int _combinedMask;

        [Inject]
        public SpacingSystem(BaseEnemyData data, IMovementActor movement)
        {
            _data = data;
            _movement = movement;

            _enemyMask = 1 << LayerMask.NameToLayer("Enemy");
            _obstacleMask = 1 << LayerMask.NameToLayer("Obstacle");
            _combinedMask = _enemyMask | _obstacleMask;
            Debug.Log("Spacing System On Line");
        }

        public void FixedTick()
        {
            // Rate-limit ¡X OverlapSphere every 0.1s is enough for avoidance
            _timer -= Time.fixedDeltaTime;
            if (_timer > 0f) return;
            _timer = TickInterval;

            _data.AvoidanceDirection = CalculateAvoidDirection();
        }

        private Vector3 CalculateAvoidDirection()
        {
            Collider[] collisions = Physics.OverlapSphere(
                _movement.Position, Radius, _combinedMask);

            if (collisions.Length == 0) return Vector3.zero;

            Vector3 avoidDirection = Vector3.zero;

            foreach (Collider col in collisions)
            {
                // Skip self : check root in case collider is on a child object
                if (col.transform.root.position == _movement.Position) continue;

                Vector3 offset = _movement.Position - col.transform.position;
                float dist = offset.magnitude;
                if (dist <= 0f) continue;

                float weight = 1f - (dist / Radius);

                // Obstacles push harder to prevent wall clipping
                if (col.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                    weight *= ObstacleWeight;

                avoidDirection += (offset / dist) * weight;
            }

            avoidDirection.y = 0f;
            return avoidDirection.normalized;
        }
    }
}
