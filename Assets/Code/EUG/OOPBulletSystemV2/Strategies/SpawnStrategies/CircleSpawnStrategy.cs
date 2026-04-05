using System.Collections.Generic;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// CircleSpawnStrategy  ── 全方向圓形彈幕
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class CircleSpawnStrategy : ISpawnStrategy
    {
        private readonly int _count;
        private readonly float _pitchAngle; // 向上/下傾斜

        public CircleSpawnStrategy(int count, float pitchAngle = 0f)
        {
            _count = count;
            _pitchAngle = pitchAngle;
        }

        public IReadOnlyList<SpawnData> GetSpawnDatas(Vector3 origin, Vector3 forward)
        {
            var result = new SpawnData[_count];
            float step = 360f / _count;

            for (int i = 0; i < _count; i++)
            {
                Quaternion rot = Quaternion.Euler(_pitchAngle, step * i, 0f);
                result[i] = new SpawnData(origin, rot * Vector3.forward);
            }

            return result;
        }
    }
}