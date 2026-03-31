using Assets.Code.EUG.AANEW.Temp;
using Nobeta.Sam.Enemy;
using Nobeta.Sam.Pathfinding;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Nobeta.Sam.Bullet;

public class MGLifetimeScope : LifetimeScope
{
    [SerializeField] private MageGhostStat _stat;
    [SerializeField] private BulletData _spellData;    // MageGhost spell asset
    [SerializeField] private Transform _muzzlePoint;  // child transform on prefab
    [SerializeField] private GameObject _barrierSphere; // child sphere ¡X toggled by summon
    [SerializeField] private Barrier _barrier;
    protected override void Configure(IContainerBuilder builder)
    {
        // ¢w¢w Stat ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        builder.RegisterInstance(_stat)
               .AsSelf()
               .As<EnemyStat>();

        // ¢w¢w Spell config ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // Injected into MGCastState directly ¡X BulletSystem resolves from parent scope.
        builder.RegisterInstance(_spellData);
        builder.RegisterInstance(_muzzlePoint);

        // ¢w¢w Runtime data ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        builder.Register<MageGhostData>(Lifetime.Scoped)
               .AsSelf()
               .As<BaseEnemyData>();

        // ¢w¢w State machine ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // Registered AsSelf so MageGhost facade can inject MGStateMachine directly.
        // AsImplementedInterfaces picks up IEnemyStateMachine, IFixedTickable,
        // IStartable, IDisposable.
        builder.Register<MGStateMachine>(Lifetime.Scoped)
               .AsSelf()
               .As<IEnemyStateMachine>()
               .AsImplementedInterfaces();

        // ¢w¢w Post-construction barrier wiring ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        // VContainer doesn't support passing a scene GameObject into a plain C# class
        // constructor cleanly, so we use a callback to set it after construction.
        builder.RegisterBuildCallback(container =>
        {
            var summon = container.Resolve<MGSummonState>();
            summon.Activate(destroyCancellationToken);
        });

        // ¢w¢w Spacing system ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        //builder.Register<SpacingSystem>(Lifetime.Scoped)
        //       .AsImplementedInterfaces();

        //builder.Register<Astar>(Lifetime.Scoped)
        //       .As<IPathFindingStrat>();

        // ¢w¢w MonoBehaviour actors ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        builder.RegisterComponent(GetComponent<MGMovementActor>())
               .AsSelf()
               .As<IMovementActor>();

        builder.RegisterComponent(GetComponentInChildren<MGAnimationActor>())
               .AsSelf()
               .As<IAnimationActor>();

        builder.RegisterComponent(GetComponentInChildren<MGPerceptionActor>())
               .AsSelf()
               .As<IPerceptionActor>();

        builder.RegisterComponent(GetComponentInChildren<Barrier>())
               .AsSelf();

        // Facade ¡X AsSelf so MGStateMachine.TriggerSummon can resolve MageGhost
        // for the destroyCancellationToken
        builder.RegisterComponent(GetComponent<MageGhost>())
               .AsSelf()
               .As<EnemyProvider>();

        // ¢w¢w Shared states ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        builder.Register<IdleState>(Lifetime.Scoped);
        //builder.Register<ChaseState>(Lifetime.Scoped);
        builder.Register<StaggerState>(Lifetime.Scoped);
        builder.Register<VictoryState>(Lifetime.Scoped);
        builder.Register<DeathState>(Lifetime.Scoped);

        // ¢w¢w MG-specific states ¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w¢w
        //builder.Register<MGCastState>(Lifetime.Scoped);
        //builder.Register<MGSummonState>(Lifetime.Scoped);
        builder.Register<MGAttackState>(Lifetime.Scoped);
        builder.Register<MGSummonState>(Lifetime.Scoped);
        builder.Register<MGSleepState>(Lifetime.Scoped);

        

        // GameObject ref for DeathState dissolve + Destroy()
        builder.RegisterInstance(gameObject);
    }
}
