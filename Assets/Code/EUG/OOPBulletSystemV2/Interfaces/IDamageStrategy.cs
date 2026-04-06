using Cysharp.Threading.Tasks;
using Nobeta.EUG.Interfaces;
// ════════════════════════════════════════════════════════════════════════
// 3. IDamageStrategy  ── 決定「打到目標後怎麼結算傷害」
// ════════════════════════════════════════════════════════════════════════
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public interface IDamageStrategy
    {
        /// <summary>
        /// 執行傷害結算（可以是單體、範圍、DoT…）
        /// </summary>
        UniTaskVoid ExecuteAsync(
            BulletState state,
            CollisionHit hit);
        BulletHitType HitType { get; }
        IDamageStrategy Clone(); // ✅
    }
}
