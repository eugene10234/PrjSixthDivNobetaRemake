using Nobeta.EUG.FSM.Services;
using Nobeta.EUG.MBFunctions.PlayerUI;
using VContainer;
using VContainer.Unity;

public class CanvasLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<PlayerStatusUI>();
        builder.RegisterComponentInHierarchy<MagicStatusUI>();
        builder.RegisterComponentInHierarchy<AimIconUI>();
        
    }
}
