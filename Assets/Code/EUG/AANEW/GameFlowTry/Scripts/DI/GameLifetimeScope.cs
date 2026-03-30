using Nobeta.EUG.GameFlowTry.GameScene;
using Nobeta.EUG.GameFlowTry.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlowTry.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerController _player;
        [SerializeField] private GameHudView _hud;
        [SerializeField] private CutscenePlayer _cutscene;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_player);
            builder.RegisterComponent(_hud);
            builder.RegisterComponent(_cutscene);
            builder.RegisterEntryPoint<GameController>();
        }
    }
}
