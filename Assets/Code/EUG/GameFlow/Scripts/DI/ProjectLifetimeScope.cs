using Nobeta.EUG.GameFlow.Core.EventBus;
using Nobeta.EUG.GameFlow.Core.Save;
using Nobeta.EUG.GameFlow.Core.Scene;
using Nobeta.EUG.GameFlow.DI.Nobeta.EUG.GameFlowTry.DI;
using Nobeta.EUG.GameFlow.Game;
using Nobeta.EUG.GameFlow.UI;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlow.DI
{
    /// <summary>
    /// DontDestroyOnLoad。全域唯一：EventBus / SaveSystem / SceneLoader / GameManager。
    /// </summary>
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingScreenView _loadingScreenView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EventBusConfig>(Lifetime.Scoped);
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
            builder.Register<SaveSystem>(Lifetime.Singleton).As<ISaveSystem>();
            if (_loadingScreenView != null)
                builder.RegisterComponent(_loadingScreenView);
            else
                Debug.LogError("[ProjectLifetimeScope] _loadingScreenView is NULL! 請在 Inspector 拖入！");
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<GameManager>(Lifetime.Singleton).As<IGameManager>();

            // ✅ EntryPoint 注入 IGameManager，強制 GameManager 立即被建構
            builder.RegisterEntryPoint<AppInitializer>();

        }
    }
}
