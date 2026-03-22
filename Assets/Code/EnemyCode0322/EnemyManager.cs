using Nobeta.Sam.Pathfinding;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    public class EnemyManager : LifetimeScope, IEnemyManager
    {
        public GridConfig gridConfig;

        [SerializeField] private BGLifetimeScope bigGhostScopePrefab; // Prefab's Scope
        public BigGhostStat bgs;
        public BigGhostSpawner spawner;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(bgs).AsSelf();
            builder.Register<BigGhostFactory>(Lifetime.Singleton)
                   .WithParameter(bigGhostScopePrefab)
                   .As<IEnemyFactory<BigGhost>>();
            builder.RegisterComponent(spawner).AsSelf().AsImplementedInterfaces();
            builder.RegisterInstance(gridConfig).AsSelf();
            builder.Register<SquareGrid>(Lifetime.Scoped).As<IGrid>().AsImplementedInterfaces();
            builder.Register<NodeData>(Lifetime.Scoped).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponent(GetComponent<GridVisual>());
        }
    }
}
