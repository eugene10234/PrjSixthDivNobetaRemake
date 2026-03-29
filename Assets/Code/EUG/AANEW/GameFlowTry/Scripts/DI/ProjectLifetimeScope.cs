using Nobeta.EUG.GameFlowTry.Core.Save;
using Nobeta.EUG.GameFlowTry.Core.Scene;
using Nobeta.EUG.GameFlowTry.Game;
using Nobeta.EUG.GameFlowTry.UI;
using Nobeta.EUG.Interfaces;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlowTry.DI
{
    /// <summary>
    /// DontDestroyOnLoad。全域唯一：EventBus / SaveSystem / SceneLoader / GameManager。
    /// </summary>
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingScreenView _loadingScreenView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EventBus>(Lifetime.Singleton).As<IEventBus>();
            builder.Register<SaveSystem>(Lifetime.Singleton).As<ISaveSystem>();
            builder.RegisterComponent(_loadingScreenView);
            builder.Register<SceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<GameManager>(Lifetime.Singleton).As<IGameManager>();
        }
    }
}
