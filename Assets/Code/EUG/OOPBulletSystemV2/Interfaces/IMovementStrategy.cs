// ════════════════════════════════════════════════════════════════════════
// 2. IMovementStrategy  ── 決定「每幀怎麼移動」
// ════════════════════════════════════════════════════════════════════════
namespace Nobeta.EUG.BulletSystemV2.OOP
{
    public interface IMovementStrategy
    {
        /// <summary>
        /// 初始化（子彈從池中取出時呼叫，可重置內部狀態）
        /// </summary>
        void OnInitialize(BulletState state);

        /// <summary>
        /// 每幀更新位置與速度，直接修改 BulletState
        /// </summary>
        void UpdateMovement(BulletState state, float deltaTime);
        IMovementStrategy Clone(); // ✅
    }
}
