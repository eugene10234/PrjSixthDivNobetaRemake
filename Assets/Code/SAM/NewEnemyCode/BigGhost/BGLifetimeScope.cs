using Assets.Code.EUG.AANEW.Temp;
using Nobeta.Sam.Enemy;
using Nobeta.Sam.Pathfinding;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BGLifetimeScope : LifetimeScope
{
    [SerializeField] private BigGhostStat _stat;

    protected override void Configure(IContainerBuilder builder)
    {
        // ¢w¢w Stat ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // RegisterInstance ¡X ScriptableObject asset, not constructed by VContainer.
        // Dual registration: BG-specific classes inject BigGhostStat,
        // shared states inject EnemyStat.
        builder.RegisterInstance(_stat)
               .AsSelf()
               .As<EnemyStat>();

        // ¢w¢w Runtime data ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // Dual registration: BG-specific classes inject BigGhostData,
        // shared states inject BaseEnemyData.
        // Both resolve to the same single Scoped instance.
        builder.Register<BigGhostData>(Lifetime.Scoped)
               .AsSelf()
               .As<BaseEnemyData>();

        // ¢w¢w State machine ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // AsImplementedInterfaces picks up:
        //   IEnemyStateMachine  ¡X injected into BigGhost facade
        //   IFixedTickable      ¡X VContainer calls FixedTick() automatically
        //   IStartable          ¡X VContainer calls Start() after all Constructs
        //   IDisposable         ¡X VContainer calls Dispose() on scope teardown
        builder.Register<BigGhostStateMachine>(Lifetime.Scoped)
               .AsSelf()
               .As<IEnemyStateMachine>()
               .AsImplementedInterfaces();

        // ¢w¢w Spacing system ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // Plain C# calculator ¡X reads neighbour positions, writes
        // AvoidanceDirection into BaseEnemyData each FixedTick.
        // IFixedTickable registration lets VContainer tick it automatically.
        builder.Register<SpacingSystem>(Lifetime.Scoped)
               .AsImplementedInterfaces();

        
        builder.Register<Astar>(Lifetime.Scoped)
               .As<IPathFindingStrat>();

        builder.RegisterComponent(GetComponent<EnemyUIController>())
               .AsSelf()
               .AsImplementedInterfaces();

        // ¢w¢w MonoBehaviour actors ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // RegisterComponent ¡X VContainer grabs the existing component,
        // never constructs a new one.
        // AsSelf         ¡÷ BG-specific injection (e.g. BGMovementActor in BGAttackState)
        // As<IInterface> ¡÷ shared state injection (e.g. IMovementActor in ChaseState)
        builder.RegisterComponent(GetComponent<BGMovementActor>())
               .AsSelf()
               .As<IMovementActor>();

        builder.RegisterComponent(GetComponentInChildren<BGAnimationActor>())
               .AsSelf()
               .As<IAnimationActor>();

        builder.RegisterComponent(GetComponentInChildren<BGPerceptionActor>())
               .AsSelf()
               .As<IPerceptionActor>();

        // Facade
        // Three registrations because three different systems reference it:
        //   AsSelf            ¡÷ BG-specific code that needs BigGhost directly
        //   As<IEnemy>        ¡÷ detection triggers, player combat system
        //   As<EnemyProvider> ¡÷ spawner system
        builder.RegisterComponent(GetComponent<BigGhost>())
               .AsSelf()
               .As<EnemyProvider>();

        // ¢w¢w Shared states ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // Each BG instance gets its own scoped copies ¡X states are stateful
        // (timers, cached values) so they must never be shared across instances.
        builder.Register<IdleState>(Lifetime.Scoped);
        builder.Register<ChaseState>(Lifetime.Scoped);
        builder.Register<StaggerState>(Lifetime.Scoped);
        builder.Register<VictoryState>(Lifetime.Scoped);
        builder.Register<DeathState>(Lifetime.Scoped);

        // ¢w¢w BG-specific states ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        builder.Register<BGAttackState>(Lifetime.Scoped);

        // GameObject ref 
        // DeathState needs this to call Destroy() after the dissolve sequence.
        builder.RegisterInstance(gameObject);
    }
}
