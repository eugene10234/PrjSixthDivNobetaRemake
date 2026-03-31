using VContainer;
using VContainer.Unity;
using Nobeta.Sam.Bullet;
using UnityEngine;
using Assets.Code.EUG.AANEW.Temp;

namespace Nobeta.Sam.Enemy
{

    public class SGLifetimeScope : LifetimeScope
    {
        [SerializeField] private SmallGhostStat _stat;
        [SerializeField] private BulletData _bulletData;
        [SerializeField] private Transform _muzzlePoint;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_bulletData)
                .AsSelf()
                .As<BulletData>();
            
            builder.RegisterInstance(_stat)
               .AsSelf()
               .As<EnemyStat>();

            builder.RegisterInstance(_muzzlePoint)
                .As<Transform>();

            builder.Register<SmallGhostData>(Lifetime.Scoped)
              .AsSelf()
              .As<BaseEnemyData>();

            builder.Register<SGStateMachine>(Lifetime.Scoped)
              .AsSelf()
              .As<IEnemyStateMachine>()
              .AsImplementedInterfaces();

            builder.Register<SpacingSystem>(Lifetime.Scoped)
               .AsImplementedInterfaces();

            builder.RegisterComponent(GetComponent<EnemyUIController>())
               .AsSelf()
               .AsImplementedInterfaces();

            builder.RegisterBuildCallback(container =>
            {
                var floatState = container.Resolve<SGIdleFloatState>();
                floatState.ActivateToken(this.destroyCancellationToken);
            });

            builder.RegisterComponent(GetComponentInChildren<SGRune>())
                .AsSelf();

            builder.RegisterComponent(GetComponentInChildren<SGPerceptionActor>())
               .AsSelf()
               .As<IPerceptionActor>();

            builder.RegisterComponent(GetComponent<SGMovementActor>())
              .AsSelf()
              .As<IMovementActor>();

            builder.RegisterComponent(GetComponentInChildren<SGAnimationActor>())
                   .AsSelf()
                   .As<IAnimationActor>();

            builder.RegisterComponent(GetComponent<SmallGhost>())
              .AsSelf()
              .As<EnemyProvider>();

            builder.Register<SGIdleFloatState>(Lifetime.Scoped);
            builder.Register<StaggerState>(Lifetime.Scoped);
            builder.Register<VictoryState>(Lifetime.Scoped);
            builder.Register<DeathState>(Lifetime.Scoped);
            // GameObject ref 
            // DeathState needs this to call Destroy() after the dissolve sequence.
            builder.RegisterInstance(gameObject);
        }
    }
}
