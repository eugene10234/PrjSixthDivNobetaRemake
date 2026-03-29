using Nobeta.EUG.GameFlowTry.GameScene;
using Nobeta.EUG.GameFlowTry.Menu;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlowTry.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameController>();
        }
    }
}
