using System.Collections.Generic;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// BurstSpawnStrategy  ── 一次同時發射 N 顆（例：5 顆火球扇形散射）
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class BurstSpawnStrategy : ISpawnStrategy
    {
        private readonly int _count;
        private readonly float _spreadAngle; // 總扇形角度（度）
        private readonly float _offsetY;     // 垂直偏移（散彈用）

        /// <param name="count">同時發射數量</param>
        /// <param name="spreadAngle">水平扇形總角度（0 = 平行發射）</param>
        /// <param name="offsetY">垂直偏移角度</param>
        public BurstSpawnStrategy(int count, float spreadAngle = 0f, float offsetY = 0f)
        {
            _count = count;
            _spreadAngle = spreadAngle;
            _offsetY = offsetY;
        }

        public IReadOnlyList<SpawnData> GetSpawnDatas(Vector3 origin, Vector3 forward)
        {
            var result = new SpawnData[_count];

            for (int i = 0; i < _count; i++)
            {
                float t = _count == 1 ? 0f : (float)i / (_count - 1) - 0.5f; // -0.5 ~ 0.5
                float yaw = t * _spreadAngle;
                float pitch = -_offsetY;

                Quaternion rot = Quaternion.LookRotation(forward)
                               * Quaternion.Euler(pitch, yaw, 0f);

                result[i] = new SpawnData(origin, rot * Vector3.forward);
            }

            return result;
        }
    }
}