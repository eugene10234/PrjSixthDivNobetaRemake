using System.Collections.Generic;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// ShotgunSpawnStrategy  ── 散彈：N 顆帶隨機偏移（保留原有 SpawnCircleShotgun 行為）
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class SkyRadiusSpawnStrategy : ISpawnStrategy
    {
        private readonly int _pellets;
        private readonly float _radius;
        private readonly float _height; // 散彈的最大偏移角度

        public SkyRadiusSpawnStrategy(int pellets, float radius , float height)
        {
            _pellets = pellets;
            _radius = radius;
            _height = height;
        }

        public IReadOnlyList<SpawnData> GetSpawnDatas(Vector3 origin, Vector3 forward)
        {
            var result = new SpawnData[_pellets];
            var baseRot = Quaternion.LookRotation(forward);

            for (int i = 0; i < _pellets; i++)
            {
                Vector2 spread = Random.insideUnitCircle * _radius;         

                //Quaternion rot = baseRot * Quaternion.Euler(spread.y, spread.x, 0f);
                origin.y = Random.Range(_height, _height*2.5f); // 根據高度參數調整生成位置的垂直位置
                origin.x += spread.x; // 根據偏移調整生成位置的水平位置
                origin.z += spread.y; // 根據偏移調整生成位置的水平位置
                result[i] = new SpawnData(origin, Vector3.down);
            }

            return result;
        }
    }
}