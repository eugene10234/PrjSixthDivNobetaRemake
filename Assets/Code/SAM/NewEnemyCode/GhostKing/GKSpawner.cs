using UnityEngine;
using VContainer;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Ghost King spawner.
    /// OnReady is intentionally left empty — the GK is not spawned on Start.
    /// Call <see cref="SpawnAt"/> from your arena interaction point / trigger volume
    /// instead (e.g. OnTriggerEnter on the player's entry collider).
    ///
    /// Example usage in an interaction script:
    /// <code>
    ///     [Inject] GKSpawner _gkSpawner;
    ///     void OnPlayerInteract() => _gkSpawner.SpawnAt(bossThrone.position, Quaternion.identity);
    /// </code>
    /// </summary>
    public class GKSpawner : EnemySpawner<GhostKing>
    {
        protected override void OnReady()
        {
            // Intentionally empty — GK is not auto-spawned on Start.
            // Wire this through your interaction / trigger system instead.
        }

        /// <summary>
        /// Convenience wrapper so callers don't have to cast to the base Create method.
        /// </summary>
        public void SpawnAt(Vector3 position, Quaternion rotation)
        {
            Create(position, rotation);
        }
    }
}
