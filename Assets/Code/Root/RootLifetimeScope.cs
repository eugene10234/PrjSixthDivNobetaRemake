using Assets.Scripts.Root;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Root
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameManager>(Lifetime.Singleton).AsSelf();
        }
    }
}
