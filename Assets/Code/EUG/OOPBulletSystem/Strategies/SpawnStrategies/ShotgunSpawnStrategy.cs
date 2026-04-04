using System.Collections.Generic;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// ShotgunSpawnStrategy  ── 散彈：N 顆帶隨機偏移（保留原有 SpawnCircleShotgun 行為）
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class ShotgunSpawnStrategy : ISpawnStrategy
    {
        private readonly int _pellets;
        private readonly float _spreadAngle;

        public ShotgunSpawnStrategy(int pellets, float spreadAngle)
        {
            _pellets = pellets;
            _spreadAngle = spreadAngle;
        }

        public IReadOnlyList<SpawnData> GetSpawnDatas(Vector3 origin, Vector3 forward)
        {
            var result = new SpawnData[_pellets];
            var baseRot = Quaternion.LookRotation(forward);

            for (int i = 0; i < _pellets; i++)
            {
                Vector2 spread = i == 0
                    ? Vector2.zero
                    : Random.insideUnitCircle * _spreadAngle;

                spread.y = Mathf.Clamp(spread.y, -_spreadAngle, _spreadAngle * 0.5f);

                Quaternion rot = baseRot * Quaternion.Euler(spread.y, spread.x, 0f);
                result[i] = new SpawnData(origin, rot * Vector3.forward);
            }

            return result;
        }
    }
}