using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Scene-level spawner for SmallGhost prefabs.
    /// Used when SmallGhosts are placed in the world directly (not summoned by MageGhost).
    /// For MageGhost summoning, inject IEnemyFactory<SmallGhost> directly into MGSummonState.
    /// </summary>
    public class SmallGhostSpawner : EnemySpawner<SmallGhost>
    {
        public List<Transform> SpawnLocations;
        public float MaxDistance;
        public float MinDistance;
        public int   NumberPerPoint;

        protected override void OnReady()
        {
            Debug.Log($"<color=green>SPAWNING SG: {SpawnLocations.Count * NumberPerPoint}</color>");

            foreach (Transform spawnPoint in SpawnLocations)
            {
                for (int i = 0; i < NumberPerPoint; i++)
                {
                    Vector2 randomCircle = Random.insideUnitCircle.normalized
                                         * Random.Range(MinDistance, MaxDistance);

                    Vector3 spawnPos = spawnPoint.position
                                     + new Vector3(randomCircle.x, 0f, randomCircle.y);

                    Create(spawnPos, Quaternion.identity);
                }
            }
        }
    }
}
