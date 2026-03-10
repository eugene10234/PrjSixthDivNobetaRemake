using Assets.Scripts.Root;
using Nobeta.EUG.FSM;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Root
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("=== 配置 Root LifetimeScope ===");

            // 輸入服務（全局單例）
            builder.Register<PlayerInputService>(Lifetime.Singleton)
                .AsImplementedInterfaces().AsSelf();
            // 推薦寫法：直接註冊類，再告訴 VContainer 它要當作哪些介面使用
            builder.Register<CameraService>(Lifetime.Singleton)
                .AsImplementedInterfaces().AsSelf();
            //事件總線（全局單例）
            builder.Register<EventBusConfig>(Lifetime.Scoped);
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            //時間服務（全局單例）
            builder.Register<ITimeService, TimeService>(Lifetime.Singleton);

            //音頻服務（全局單例）
            builder.Register<IAudioService, AudioService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<GameManager>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<DebugServiceMb>();
            Debug.Log("Root LifetimeScope 配置完成");

        }
    }
}
