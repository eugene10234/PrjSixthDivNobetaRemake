using Assets.Code.EUG.AANEW.Temp;
using Cysharp.Threading.Tasks;
using Nobeta.EUG.Interfaces;
using System.Threading;
using UnityEngine;
// ─────────────────────────────────────────────────────────────────────────
// DotDamageStrategy  ── 持續傷害（燃燒 / 毒 / 冰凍減速）
//
// UniTask 在此的價值：
//   tick 間隔是「時序問題」，不是架構問題，用 UniTask 最乾淨。
//   避免在 Update loop 裡管理 N 個 DoT 計時器。
// ─────────────────────────────────────────────────────────────────────────
namespace Nobeta.EUG.BulletSystemV2.OOP
{ 
    public sealed class DotDamageStrategy : DamageStrategyBase
    {
        public override BulletHitType HitType => BulletHitType.Dot;
        private readonly IEventBus _eventBus;  // ✅ 建構時注入，不從 Execute 傳
        private readonly int _tickCount;      // 觸發幾次
        private readonly float _tickInterval;   // 間隔（秒）
        private readonly float _damagePerTick;  // 每次傷害

        /// <param name="tickCount">觸發總次數</param>
        /// <param name="tickInterval">每次間隔秒數</param>
        /// <param name="damagePerTick">每次傷害量（0 = 使用 state.Damage / tickCount）</param>
        public DotDamageStrategy(IEventBus eventBus, int tickCount = 5, float tickInterval = 1f, float damagePerTick = 0f)
        {
            _eventBus = eventBus;
            _tickCount = tickCount;
            _tickInterval = tickInterval;
            _damagePerTick = damagePerTick;
        }

        public override UniTaskVoid ExecuteAsync(
            BulletState state,
            CollisionHit hit)
        {
            if (_eventBus==null)
            { 
                Debug.LogError("DotDamageStrategy requires a non-null IEventBus instance. Please provide it via the constructor.");
                return default;
            }               
            // 立刻 fire-and-forget，不 await（Bullet 本身已設為 IsActive = false）
            RunDotAsync(state, hit, _eventBus, this).Forget();
            return default;
        }

        private static async UniTaskVoid RunDotAsync(
            BulletState state,
            CollisionHit hit,
            IEventBus eventBus,
            DotDamageStrategy self)
        {
            // 嘗試取得目標（目標可能在 DoT 過程中死亡）
            if (!EnemyDamageSystem.TryGetTarget(hit.GameObject, out IDamageable target))
                return;

            float dmgPerTick = self._damagePerTick > 0f
                ? self._damagePerTick
                : (float)state.Damage / self._tickCount;

            //// 立即通知 VFX 播放 DoT 特效
            //eventBus.Publish(new BulletHitEvent
            //{
            //    BulletState = state,
            //    TargetGameObject = hit.GameObject,
            //    HitPosition = state.Position,
            //    HitNormal = hit.Normal,
            //    Damage = state.Damage,
            //    Type = state.Type,
            //    HitType = BulletHitType.Dot
            //});

            for (int i = 0; i < self._tickCount; i++)
            {
                // 等待一個 tick 間隔
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(self._tickInterval),
                    cancellationToken: hit.GameObject != null
                        ? hit.GameObject.GetCancellationTokenOnDestroy()
                        : CancellationToken.None
                );

                // 目標在等待期間被銷毀
                if (hit.GameObject == null || !hit.GameObject.activeInHierarchy)
                    break;

                // 再次確認目標還活著
                if (!EnemyDamageSystem.TryGetTarget(hit.GameObject, out target))
                    break;

                target.TakeDamage(Mathf.RoundToInt(dmgPerTick), state.Position);

                // 每 tick 發布更新事件（UI / 飄字用）
                eventBus.Publish(new BulletDotTickEvent
                {
                    TargetGameObject = hit.GameObject,
                    Damage = Mathf.RoundToInt(dmgPerTick),
                    TickIndex = i,
                    TotalTicks = self._tickCount,
                    Type = state.Type
                });
            }
        }
    }
}
