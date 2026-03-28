using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Spawner for MageGhost prefabs.
    /// Extends EnemySpawner generic so the spawner system stays type-aware.
    /// </summary>
    public class MageGhostSpawner : EnemySpawner<MageGhost>
    {
        public List<Transform> SpawnLocations;
        public float MaxDistance;
        public float MinDistance;
        public int   NumberPerPoint;

        protected override void OnReady()
        {
            Debug.Log($"<color=green>SPAWNING: {SpawnLocations.Count * NumberPerPoint}</color>");

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
