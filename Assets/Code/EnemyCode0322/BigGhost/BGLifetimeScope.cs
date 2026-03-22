using Assets.Code.EUG.AANEW.Temp;
using Nobeta.Sam.Pathfinding;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    public class BGLifetimeScope : LifetimeScope
    {

        protected override void Configure(IContainerBuilder builder)
        {
            // Logic unique to this specific ghost instance
            builder.Register<BigGhostStateMachine>(Lifetime.Scoped)
                .As<IEnemyStateMachine>()
                .AsImplementedInterfaces();

            // Pathfinding unique to this ghost
            builder.Register<Astar>(Lifetime.Scoped).As<IPathFindingStrat>();

            // Register components already on this Prefab

            builder.RegisterComponent(GetComponent<BigGhost>())
            .AsSelf()
            .As<IEnemy>()
            .As<EnemyProvider>();
            builder.RegisterComponent(GetComponent<BigGhostActor>());
        }
    }
}