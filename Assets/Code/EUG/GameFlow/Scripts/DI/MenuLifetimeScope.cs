using Nobeta.EUG.GameFlow.Menu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.EUG.GameFlow.DI
{
    public class MenuLifetimeScope : LifetimeScope
    {
        [SerializeField] private MenuView _menuView;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_menuView);
            builder.RegisterEntryPoint<MenuController>();
        }
    }
}
