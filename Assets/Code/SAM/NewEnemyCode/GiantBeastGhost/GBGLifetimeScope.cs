using Assets.Code.EUG.AANEW.Temp;
using Nobeta.Sam.Enemy;
using Nobeta.Sam.Pathfinding;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GBGLifetimeScope : LifetimeScope
{
    [SerializeField] private GiantBeastGhostStat _stat;

    protected override void Configure(IContainerBuilder builder)
    {
        // ¢w¢w Stat ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // RegisterInstance = ScriptableObject asset, not constructed by VContainer.
        // Dual registration: GBG-specific classes inject GiantBeastGhostStat,
        // shared states inject EnemyStat.
        builder.RegisterInstance(_stat)
               .AsSelf()
               .As<EnemyStat>();

        // ¢w¢w Runtime data ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // Dual registration: GBG-specific classes inject GiantBeastGhostData,
        // shared states inject BaseEnemyData.
        // Both resolve to the same single Scoped instance.
        builder.Register<GiantBeastGhostData>(Lifetime.Scoped)
               .AsSelf()
               .As<BaseEnemyData>();

        // ¢w¢w State machine ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // AsImplementedInterfaces picks up:
        //   IEnemyStateMachine  = injected into GiantBeastGhost facade
        //   IFixedTickable      = VContainer calls FixedTick() automatically
        //   IStartable          = VContainer calls Start() after all Constructs
        //   IDisposable         = VContainer calls Dispose() on scope teardown
        builder.Register<GBGStateMachine>(Lifetime.Scoped)
               .AsSelf()
               .As<IEnemyStateMachine>()
               .AsImplementedInterfaces();

        // ¢w¢w Spacing system ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // Plain C# calculator ¡X reads neighbour positions, writes
        // AvoidanceDirection into BaseEnemyData each FixedTick.
        // IFixedTickable registration lets VContainer tick it automatically.
        builder.Register<SpacingSystem>(Lifetime.Scoped)
               .AsImplementedInterfaces();

        builder.Register<Astar>(Lifetime.Scoped)
               .As<IPathFindingStrat>();

        // ¢w¢w MonoBehaviour actors ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // RegisterComponent = VContainer grabs the existing component,
        // never constructs a new one.
        // AsSelf         ¡÷ GBG-specific injection (e.g. GBGMovementActor in GBGAttackState)
        // As<IInterface> ¡÷ shared state injection (e.g. IMovementActor in ChaseState)
        builder.RegisterComponent(GetComponent<GBGMovementActor>())
               .AsSelf()
               .As<IMovementActor>();

        builder.RegisterComponent(GetComponentInChildren<GBGAnimationActor>())
               .AsSelf()
               .As<IAnimationActor>();

        builder.RegisterComponent(GetComponentInChildren<GBGPerceptionActor>())
               .AsSelf()
               .As<IPerceptionActor>();

        // Facade
        // Three registrations because three different systems reference it:
        //   AsSelf            ¡÷ GBG-specific code that needs GiantBeastGhost directly
        //   As<EnemyProvider> ¡÷ spawner system
        builder.RegisterComponent(GetComponent<GiantBeastGhost>())
               .AsSelf()
               .As<EnemyProvider>();

        // ¢w¢w Shared states ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // Each GBG instance gets its own scoped copies ¡X states are stateful
        // (timers, cached values) so they must never be shared across instances.
        builder.Register<IdleState>(Lifetime.Scoped);
        builder.Register<ChaseState>(Lifetime.Scoped);
        builder.Register<StaggerState>(Lifetime.Scoped);
        builder.Register<VictoryState>(Lifetime.Scoped);
        builder.Register<DeathState>(Lifetime.Scoped);

        // ¢w¢w GBG-specific states ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        builder.Register<GBGAttackState>(Lifetime.Scoped);
        builder.Register<GBGDodgeState>(Lifetime.Scoped);

        // GameObject ref
        // DeathState needs this to call Destroy() after the dissolve sequence.
        builder.RegisterInstance(gameObject);
    }
}
