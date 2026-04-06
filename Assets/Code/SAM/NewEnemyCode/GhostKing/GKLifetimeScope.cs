using Assets.Code.EUG.AANEW.Temp;
using Nobeta.Sam.Bullet;
using Nobeta.Sam.Enemy;
using Nobeta.Sam.Pathfinding;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Ghost King child scope — one instance per GK prefab.
///
/// Resolves from parent (EnemyManager):
///   NodeData, IGrid, GridConfig, EnemyBulletSystem, SmallGhostFactory
///
/// SmallGhostFactory is resolved from the parent scope so GKSummonState
/// can spawn minions without owning the factory itself.
/// </summary>
public class GKLifetimeScope : LifetimeScope
{
    [SerializeField] private NewGKStat _stat;
    [SerializeField] private BulletData _spellData;

    protected override void Configure(IContainerBuilder builder)
    {
        // ── Stat ──────────────────────────────────────────────────────────────
        builder.RegisterInstance(_stat)
               .AsSelf()
               .As<EnemyStat>();
        builder.RegisterInstance(_spellData)
                .AsSelf()
                .As<BulletData>();
        builder.RegisterComponent(GetComponentInChildren<Barrier>())
               .AsSelf();

        // ── Pathfinding (per-instance strategy) ───────────────────────────────
        builder.Register<Astar>(Lifetime.Scoped)
               .As<IPathFindingStrat>();

        // ── Runtime data ──────────────────────────────────────────────────────
        builder.Register<GhostKingData>(Lifetime.Scoped)
               .AsSelf()
               .As<BaseEnemyData>();

        // ── State machine ─────────────────────────────────────────────────────
        builder.Register<GKStateMachine>(Lifetime.Scoped)
               .AsSelf()
               .As<IEnemyStateMachine>()
               .AsImplementedInterfaces();

        // ── MonoBehaviour actors ──────────────────────────────────────────────
        builder.RegisterComponent(GetComponent<GKMovementActor>())
               .AsSelf()
               .As<IMovementActor>();

        builder.RegisterComponent(GetComponentInChildren<GKAnimationActor>())
               .AsSelf()
               .AsImplementedInterfaces();

        builder.RegisterComponent(GetComponentInChildren<GKPerceptionActor>()) 
               .AsSelf()
               .As<IPerceptionActor>();

        // ── Facade ────────────────────────────────────────────────────────────
        builder.RegisterComponent(GetComponent<GhostKing>())
               .AsSelf()
               .As<EnemyProvider>();

        // ── Shared states ─────────────────────────────────────────────────────
        builder.Register<VictoryState>(Lifetime.Scoped);
        builder.Register<DeathState>(Lifetime.Scoped);

        // ── GK-specific states ────────────────────────────────────────────────
        builder.Register<GKSleepState>(Lifetime.Scoped);
        builder.Register<GKChaseState>(Lifetime.Scoped);
        builder.Register<GKAttackState>(Lifetime.Scoped);
        builder.Register<GKSummonState>(Lifetime.Scoped);
        builder.Register<GKTransitionState>(Lifetime.Scoped);
        builder.Register<GKStaggerState>(Lifetime.Scoped);

        // SmallGhostFactory is resolved from the parent EnemyManager scope
        // automatically — no re-registration needed here.
        // GKSummonState can inject IEnemyFactory<SmallGhost> or SmallGhostFactory directly.

        // ── GameObject ref for DeathState ─────────────────────────────────────
        builder.RegisterInstance(gameObject);
    }
}
