using Assets.Code.EUG.AANEW.Temp;
using Nobeta.Sam.Pathfinding;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    public class FireDemonLifeTimeScope : LifetimeScope
    {
        [SerializeField] private FDStat _stat;

        protected override void Configure(IContainerBuilder builder)
        {
            // ¢w¢w Stat ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
            // RegisterInstance ¡X ScriptableObject asset, not constructed by VContainer.
            // Dual registration: BG-specific classes inject BigGhostStat,
            // shared states inject EnemyStat.
            builder.RegisterInstance(_stat)
                   .AsSelf()
                   .As<EnemyStat>();

            //Runtime data
            builder.Register<FDData>(Lifetime.Scoped)
                   .AsSelf()
                   .As<BaseEnemyData>();

            // ¢w¢w State machine ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
            // AsImplementedInterfaces picks up:
            //   IEnemyStateMachine  ¡X injected into BigGhost facade
            //   IFixedTickable      ¡X VContainer calls FixedTick() automatically
            //   IStartable          ¡X VContainer calls Start() after all Constructs
            //   IDisposable         ¡X VContainer calls Dispose() on scope teardown
            builder.Register<FDStateMachine>(Lifetime.Scoped)
                   .AsSelf()
                   .As<IEnemyStateMachine>()
                   .AsImplementedInterfaces();

            // ¢w¢w Spacing system ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
            // Plain C# calculator ¡X reads neighbour positions, writes
            // AvoidanceDirection into BaseEnemyData each FixedTick.
            // IFixedTickable registration lets VContainer tick it automatically.
            builder.Register<SpacingSystem>(Lifetime.Scoped)
                   .AsImplementedInterfaces();

            // ¢w¢w Pathfinding ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
            // Scoped so each ghost instance has its own Astar with isolated state.
            // NodeData and IGrid are resolved from the parent EnemyManager scope.
            builder.Register<Astar>(Lifetime.Scoped)
                   .As<IPathFindingStrat>();

            // MonoBehaviour actors 
            // RegisterComponent ¡X VContainer grabs the existing component,
            // never constructs a new one.
            // AsSelf         ¡÷ BG-specific injection (e.g. BGMovementActor in BGAttackState)
            // As<IInterface> ¡÷ shared state injection (e.g. IMovementActor in ChaseState)
            builder.RegisterComponent(GetComponent<FDMovementActor>())
                   .AsSelf()
                   .As<IMovementActor>();

            builder.RegisterComponent(GetComponentInChildren<FDAnimationActor>())
                   .AsSelf()
                   .As<IAnimationActor>();

            builder.RegisterComponent(GetComponentInChildren<FDPerceptionActor>())
                   .AsSelf()
                   .As<IPerceptionActor>();

            // Facade
            // Three registrations because three different systems reference it:
            //   AsSelf            ¡÷ BG-specific code that needs BigGhost directly
            //   As<IEnemy>        ¡÷ detection triggers, player combat system
            //   As<EnemyProvider> ¡÷ spawner system
            builder.RegisterComponent(GetComponent<FireDemon>())
                   .AsSelf()
                   .As<EnemyProvider>();

            // ¢w¢w Shared states ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
            // (timers, cached values) so they must never be shared across instances.
            builder.Register<IdleState>(Lifetime.Scoped);
            builder.Register<ChaseState>(Lifetime.Scoped);
            builder.Register<StaggerState>(Lifetime.Scoped);
            builder.Register<VictoryState>(Lifetime.Scoped);
            builder.Register<DeathState>(Lifetime.Scoped);

            // ¢w¢w BG-specific states ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
            builder.Register<FDAttackState>(Lifetime.Scoped);

            // GameObject ref 
            // DeathState needs this to call Destroy() after the dissolve sequence.
            builder.RegisterInstance(gameObject);
        }
    }
}
