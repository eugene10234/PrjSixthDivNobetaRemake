using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Contract for the perception actor of any enemy type.
    /// Responsible for detection and line-of-sight checks only — no movement, no animation.
    /// </summary>
    public interface IPerceptionActor
    {
        /// <summary>
        /// Scan for the player within detectionRadius.
        /// Returns the player GameObject if found, null otherwise.
        /// </summary>
        GameObject ScanForPlayer(float detectionRadius);

        /// <summary>
        /// Returns true if there is unobstructed line of sight to target.
        /// Uses a SphereCast — ignores Enemy and Obstacle layers.
        /// </summary>
        bool HasLineOfSight(Vector3 target);

        /// <summary>
        /// Returns a jittered world position near the target — used as movement destination
        /// to avoid all enemies clustering on the exact same point.
        /// Updated on a fixed interval; safe to call every FixedTick.
        /// </summary>
        Vector3 GetApproximateTargetPosition(Vector3 exactTarget, float attackRange);

        Vector3 InformSpawnLocation();
    }
}
