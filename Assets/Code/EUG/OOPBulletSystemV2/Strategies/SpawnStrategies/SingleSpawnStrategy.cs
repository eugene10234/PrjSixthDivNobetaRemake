using System.Collections.Generic;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// SingleSpawnStrategy  ── 單顆子彈（原有行為，向前一顆）
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class SingleSpawnStrategy : ISpawnStrategy
    {
        public static readonly SingleSpawnStrategy Default = new();

        public IReadOnlyList<SpawnData> GetSpawnDatas(Vector3 origin, Vector3 forward)
        {
            return new[] { new SpawnData(origin, forward) };
        }
    }
}