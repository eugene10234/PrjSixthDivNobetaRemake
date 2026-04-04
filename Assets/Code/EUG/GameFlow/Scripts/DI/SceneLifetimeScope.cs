using Nobeta.EUG.GameFlow.GameScene;
using Nobeta.EUG.GameFlow.UI;
using Nobeta.EUG.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlow.DI
{
    public class SceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerTeleporter _player;
        [SerializeField] private GameHudView _hud;
        [SerializeField] private CutscenePlayer _cutscene;
        [SerializeField] private SceneSpawnPoint _sceneSpawnPoint;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<CameraService>(Lifetime.Scoped);
            builder.RegisterComponent(_player);
            builder.RegisterComponent(_hud);
            builder.RegisterComponent(_cutscene);
            builder.RegisterComponent(_sceneSpawnPoint);
            builder.RegisterEntryPoint<GameSceneController>();
        }
    }
}
