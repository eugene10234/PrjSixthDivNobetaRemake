using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    public class BigGhostSpawner :EnemySpawner<BigGhost>
    {
        public List<Transform> SpawnLocation;
        public float maxDistance;
        public float minDistance;
        public int number;
        protected override void OnReady()
        {
            Debug.Log($"<color=green>SPAWNING: {SpawnLocation.Count*number}");
            foreach (Transform spawnPoint in SpawnLocation)
            {
                for (int i = 0; i < number; i++)
                {
                    Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance);

                    Vector3 spawnLoc = spawnPoint.position + new Vector3(randomCircle.x, 0, randomCircle.y);

                    Create(spawnLoc, Quaternion.identity);
                }
            }
        }
    }
}

