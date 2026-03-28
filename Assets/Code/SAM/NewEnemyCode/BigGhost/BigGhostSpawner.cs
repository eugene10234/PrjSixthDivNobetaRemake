using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Spawner for BigGhost prefabs.
    /// Unchanged from the original except it now references BigGhost through
    /// the EnemySpawner generic so the spawner system stays type-aware.
    /// </summary>
    public class BigGhostSpawner : EnemySpawner<BigGhost>
    {
        [SerializeField] private List<BGLifetimeScope> _variantPrefabs; // drag in Forest, Cave, Snow variants etc.
        [SerializeField] private List<Transform> SpawnLocations;
        [SerializeField] private float MaxDistance;
        [SerializeField] private float MinDistance;
        [SerializeField] private int NumberPerPoint;
        [SerializeField] private int SpawnsPerFrame = 3; // New: Throttle limit

        private BigGhostFactory _factory;

        [Inject]
        public void Construct(BigGhostFactory factory)
        {
            _factory = factory;
        }

        //1 location, 1 prefab
        protected override async void OnReady()
        {
            int totalSpawnedInThisFrame = 0;

            for (int s = 0; s < SpawnLocations.Count; s++)
            {
                Transform spawnPoint = SpawnLocations[s];

                for (int i = 0; i < NumberPerPoint; i++)
                {
                    Vector3 spawnPos = CalculateSpawnPos(spawnPoint); // Helper for your math
                    BGLifetimeScope prefab = _variantPrefabs[s];

                    // Spawn the ghost
                    await _factory.CreateAsync(spawnPos, Quaternion.identity, prefab);

                    totalSpawnedInThisFrame++;

                    // If we hit our budget, wait for the next frame
                    if (totalSpawnedInThisFrame >= SpawnsPerFrame)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update);
                        totalSpawnedInThisFrame = 0;
                    }
                }
            }
        }
        private Vector3 CalculateSpawnPos(Transform point)
        {
            float distance = Random.Range(MinDistance, MaxDistance);
            Vector2 offset = Random.insideUnitCircle.normalized * distance;
            return point.position + new Vector3(offset.x, 0f, offset.y);
        }
    }
}
