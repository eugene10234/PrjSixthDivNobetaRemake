
using Nobeta.EUG.BulletSystemV2.OOP;
using Nobeta.EUG.BulletSystemV2.OOP.UnityBridge;
using Nobeta.EUG.FSM;
using Nobeta.EUG.Settings;
using System.Collections.Generic;
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
    //[RequireComponent(typeof(BulletSystemBridgeV2))]
    public class BulletSystemScopeV2 : LifetimeScope
    {
        //[SerializeField] private Transform firePoint;
        [SerializeField] private BulletVFXConfig[] bulletVFXConfigs;
        protected override void Awake()
        {
            // 驗證配置
            if (bulletVFXConfigs == null)
            {
                Debug.LogError("bulletVFXConfigs 未設置！");
            }
            //if (firePoint == null)
            //{
            //    Debug.LogError("firePoint 未設置！");
            //}
            base.Awake();
        }
        protected override void Configure(IContainerBuilder builder)
        {
            if (bulletVFXConfigs != null)
            {
                builder.RegisterInstance(bulletVFXConfigs);
            }
            //if (firePoint != null)
            //{
            //    builder.RegisterInstance(firePoint);
            //}
            builder.RegisterInstance(transform);
            // 物理服務
            builder.Register<IPhysicsService, PhysicsService>(Lifetime.Singleton);

            // 子彈工廠（Transient Bullet + Singleton Factory）
            builder.Register<Bullet>(Lifetime.Transient);
            builder.Register<BulletFactory>(Lifetime.Singleton);
            //builder.Register<BulletConfigFactory>(Lifetime.Singleton);
            //builder.Register<BulletConfigRegistry>(Lifetime.Singleton);
            builder.Register<BulletManager>(Lifetime.Singleton);
            //builder.Register<PlayerWeaponSystemV2>(Lifetime.Singleton);
            
            builder.RegisterEntryPoint<BulletSystemPresenterV2>().As<BulletSystemPresenterV2>();
            // Unity Bridge
            //builder.RegisterComponentInHierarchy<BulletSystemBridgeV2>();
            builder.RegisterComponentInHierarchy<PlayerWeaponSystemV2>();

            Debug.Log("[BulletSystem v2] VContainer 註冊完成");
        }
    }
}
