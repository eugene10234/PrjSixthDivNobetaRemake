using Cysharp.Threading.Tasks;
using Nobeta.EUG.Interfaces;
// ════════════════════════════════════════════════════════════════════════
// 3. IDamageStrategy  ── 決定「打到目標後怎麼結算傷害」
// ════════════════════════════════════════════════════════════════════════
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public abstract class DamageStrategyBase : IDamageStrategy
    {
        public virtual IDamageStrategy Clone()
        {
            return (IDamageStrategy)MemberwiseClone();
        }
        public abstract BulletHitType HitType { get; }  // ✅ Strategy 聲明自己的類型
        public abstract UniTaskVoid ExecuteAsync(BulletState state, CollisionHit hit);
    }
}
