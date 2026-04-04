using System.Collections.Generic;
using UnityEngine;
// ════════════════════════════════════════════════════════════════════════
// 1. ISpawnStrategy  ── 決定「生成幾顆、往哪飛」
// ════════════════════════════════════════════════════════════════════════
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public interface ISpawnStrategy
    {
        /// <summary>
        /// 回傳本次要發射的所有 SpawnData（位置 + 方向）
        /// </summary>
        IReadOnlyList<SpawnData> GetSpawnDatas(Vector3 origin, Vector3 forward);
    }

    public readonly struct SpawnData
    {
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        public SpawnData(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
        }
    }
}
