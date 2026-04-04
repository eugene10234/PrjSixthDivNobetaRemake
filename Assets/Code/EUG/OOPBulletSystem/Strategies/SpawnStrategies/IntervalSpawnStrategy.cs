using System.Collections.Generic;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// IntervalSpawnStrategy  ── 定時生成（例：每 2 秒自動補一顆火球）
//
// 注意：此 Strategy 本身不持有計時器 ── 計時器由呼叫方（BulletManager）管理。
// 它只負責「當計時到了，給我 SpawnData」。
// 真正的「每 2 秒」排程由 BulletConfig.SpawnInterval + BulletManager 負責。
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public sealed class IntervalSpawnStrategy : ISpawnStrategy
    {
        public float Interval { get; }
        private readonly ISpawnStrategy _inner; // 每次觸發時實際生成的形狀

        /// <param name="interval">觸發間隔（秒）</param>
        /// <param name="inner">每次觸發時使用的生成策略（預設 Single）</param>
        public IntervalSpawnStrategy(float interval, ISpawnStrategy inner = null)
        {
            Interval = interval;
            _inner = inner ?? SingleSpawnStrategy.Default;
        }

        public IReadOnlyList<SpawnData> GetSpawnDatas(Vector3 origin, Vector3 forward)
            => _inner.GetSpawnDatas(origin, forward);
    }
}