using Nobeta.EUG.GameFlowTry.Menu;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlowTry.DI
{
    public class MenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MenuController>();
        }
    }
}
