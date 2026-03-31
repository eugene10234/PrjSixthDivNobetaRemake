using Nobeta.EUG.FSM;
using Nobeta.EUG.FSM.Interfaces;
using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.GameFlowTry.Core.Save;
using Nobeta.EUG.GameFlowTry.Core.Scene;
using Nobeta.EUG.GameFlowTry.DI.Nobeta.EUG.GameFlowTry.DI;
using Nobeta.EUG.GameFlowTry.Game;
using Nobeta.EUG.GameFlowTry.UI;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Root
{

    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingScreenView _loadingScreenView;
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("=== 配置 Root LifetimeScope ===");

            // 輸入服務（全局單例）
            builder.Register<PlayerInputService>(Lifetime.Singleton)
                .AsImplementedInterfaces().AsSelf();
            // 推薦寫法：直接註冊類，再告訴 VContainer 它要當作哪些介面使用
            builder.Register<CameraService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            //事件總線（全局單例）
            builder.Register<EventBusConfig>(Lifetime.Scoped);
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);

            //時間服務（全局單例）
            builder.Register<ITimeService, TimeService>(Lifetime.Singleton);

            //音頻服務（全局單例）
            builder.Register<IAudioService, AudioService>(Lifetime.Singleton);

            builder.Register<SaveSystem>(Lifetime.Singleton).As<ISaveSystem>();
            if (_loadingScreenView != null)
                builder.RegisterComponent(_loadingScreenView);
            else
                Debug.LogError("[RootLifetimeScope] _loadingScreenView is NULL! 請在 Inspector 拖入！");
            //builder.RegisterComponentInHierarchy<LoadingScreenView>();
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<GameManager>(Lifetime.Singleton).As<IGameManager>();

            //  EntryPoint 注入 IGameManager，強制 GameManager 立即被建構
            builder.RegisterEntryPoint<AppInitializer>();
            //builder.RegisterEntryPoint<GameManager>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<DebugServiceMb>();
            builder.RegisterComponentInHierarchy<EventBusProvider>();
            Debug.Log("Root LifetimeScope 配置完成");           
        }
    }
}
