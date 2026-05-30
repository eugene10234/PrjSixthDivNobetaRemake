using GDCO.Core.Entities;
using GDCO.Combat.Damage;
using GDCO.Combat.Integration;
using VContainer;
using VContainer.Unity;

namespace GDCO.Installers
{
    /// <summary>
    /// Combat 系統的 VContainer Installer
    /// 註冊所有依賴
    /// </summary>
    public class CombatInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // ===== Core Systems =====
            
            // Entity ID 生成器 - Singleton
            builder.Register<EntityIdProvider>(Lifetime.Singleton)
                .As<IEntityIdProvider>();
            
            // Entity Registry - Singleton
            builder.Register<EntityRegistry>(Lifetime.Singleton)
                .As<IEntityRegistry>();

            // EventBus - 假設已在其他地方註冊
            // 如果沒有，取消註解以下行：
            // builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();

            // ===== Combat Systems =====
            
            // Damage System - Singleton
            builder.Register<DamageSystem>(Lifetime.Singleton)
                .As<IDamageSystem>();

            // Collision Registry - Singleton (Hot Path 優化)
            builder.Register<EnemyCollisionRegistry>(Lifetime.Singleton)
                .As<IEnemyCollisionRegistry>();

            // Enemy Spawner - Singleton
            builder.Register<EnemySpawner>(Lifetime.Singleton)
                .As<IEnemySpawner>();

   
        }
    }
}
