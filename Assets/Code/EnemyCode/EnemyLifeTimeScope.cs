using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    public class EnemyLifetimeScope : LifetimeScope
    {
        //[SerializeField] private BigGhost bG;
        [SerializeField] private TestBullet _bulletInScene;
        [SerializeField] private BigGhost bigGhostPrefab;

        public BigGhostStat bgs;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<EnemyManager>(Lifetime.Singleton).As<IEnemyManager>();
            builder.Register<EnemyVisuals>(Lifetime.Scoped).AsSelf();
            builder.RegisterComponent(_bulletInScene)
                        .AsSelf();
            
            
            //BigGhostDependencies: Stats. Factory. Spawner.
            builder.RegisterInstance(bgs).AsSelf();
            builder.Register<BigGhostFactory>(Lifetime.Singleton)
           .WithParameter(bigGhostPrefab)
           .As<IEnemyFactory<BigGhost>>();
            builder.RegisterComponentInHierarchy<BigGhostSpawner>();

            // TEST DUMMY
            //builder.RegisterComponent(bG).AsSelf();
        }
    }
}