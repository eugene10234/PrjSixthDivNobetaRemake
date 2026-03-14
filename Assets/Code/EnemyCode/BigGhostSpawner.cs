using UnityEngine;

namespace Nobeta.Sam.Enemy
{
    public class BigGhostSpawner :EnemySpawner<BigGhost>
    {
        protected override void OnReady()
        {
            Vector3 spawnLoc = gameObject.transform.position;
            Create(spawnLoc, Quaternion.identity);
        }
    }
}
