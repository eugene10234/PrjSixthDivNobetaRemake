using Nobeta.EUG.BulletSystem.OOP;
using Nobeta.EUG.BulletSystem.OOP.UnityBridge;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;
// ========================================================================
// VContainer Scope Configuration
// ========================================================================
namespace Nobeta.EUG.BulletSystem
{
    /// <summary>
    /// 子彈系統 Lifetime Scope
    /// </summary>
    public class BulletSystemScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // 註冊 EventBus（如果還沒註冊）
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            // 註冊服務
            builder.Register<IPhysicsService, PhysicsService>(Lifetime.Singleton);

            // 註冊工廠
            builder.Register<BulletFactory>(Lifetime.Singleton);

            // 註冊管理器
            builder.Register<BulletManager>(Lifetime.Singleton);

            // 註冊 Bullet（Transient - 每次創建新的）
            builder.Register<Bullet>(Lifetime.Transient);

            builder.RegisterComponentInHierarchy<BulletSystemBridge>();
            builder.RegisterComponentInHierarchy<WeaponSystem>();

            Debug.Log("[BulletSystem] VContainer 註冊完成");
        }
    }
}
