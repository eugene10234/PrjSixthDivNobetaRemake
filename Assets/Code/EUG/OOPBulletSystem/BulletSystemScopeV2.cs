
using Nobeta.EUG.BulletSystemV2.OOP;
using Nobeta.EUG.BulletSystemV2.OOP.UnityBridge;
using UnityEngine;
using VContainer;
using VContainer.Unity;
// ============================================================================
// BulletSystemScope v2  ── VContainer 綁定
// Strategy 是 Plain C# 物件，由 BulletConfig 持有，不需要在 Scope 額外註冊。
// ============================================================================
namespace Nobeta.EUG.BulletSystemV2
{
    /// <summary>
    /// 子彈系統 Lifetime Scope
    /// </summary>
    public class BulletSystemScopeV2 : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 物理服務
            builder.Register<IPhysicsService, PhysicsService>(Lifetime.Singleton);

            // 子彈工廠（Transient Bullet + Singleton Factory）
            builder.Register<Bullet>(Lifetime.Transient);
            builder.Register<BulletFactory>(Lifetime.Singleton);

            // 子彈管理器
            builder.Register<BulletManager>(Lifetime.Singleton);

            // Unity Bridge
            builder.RegisterComponentInHierarchy<BulletSystemBridgeV2>();
            builder.RegisterComponentInHierarchy<PlayerWeaponSystemV2>();

            Debug.Log("[BulletSystem v2] VContainer 註冊完成");
        }
    }
}
