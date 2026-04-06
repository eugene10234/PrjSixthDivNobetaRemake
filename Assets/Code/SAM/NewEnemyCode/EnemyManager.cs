using Nobeta.Sam.Bullet;
using Nobeta.Sam.Pathfinding;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Nobeta.Sam.Enemy
{
    /// <summary>
    /// Root DI scope for the enemy system.
    /// Owns everything that is shared across ALL enemy instances:
    ///   - Grid and pathfinding infrastructure (NodeData, IGrid, GridConfig)
    ///   - Factories and spawners
    ///
    /// Deliberately does NOT own:
    ///   - BigGhostStat — each BGLifetimeScope carries its own via [SerializeField]
    ///   - Any per-instance state — that lives in child scopes (BGLifetimeScope, etc.)
    ///
    /// Scope hierarchy:
    ///   EnemyManager (parent)
    ///       └── BGLifetimeScope (child, one per BigGhost prefab instance)
    ///       └── MGLifetimeScope (child, one per MageGhost prefab instance)  ← future
    ///       └── SGLifetimeScope (child, one per SmallGhost prefab instance) ← future
    ///
    /// Child scopes resolve NodeData and IGrid from this parent scope automatically.
    /// Nothing in this scope resolves anything from a child scope.
    /// </summary>
    public class EnemyManager : LifetimeScope
    {
        // ── Inspector fields ──────────────────────────────────────────────────

        [SerializeField] private GridConfig      _gridConfig;

        //[Header("Bullet System")]
        //[SerializeField] private BulletSystemRoot _bulletSystemRoot;

        // The prefab scope reference used by the factory to instantiate BG prefabs.
        // This is the BGLifetimeScope prefab, NOT a stat asset — stat lives on the prefab.
        [Header("BigGhost")]
        [SerializeField] private BigGhostSpawner _bgspawner;

        [Header("FireDemon")]
        [SerializeField] private FireDemonLifeTimeScope _firedemonScopePrefab;
        [SerializeField] private FireDemonSpawner _fdspawner;

        [Header("GiantBeastGhost")]
        [SerializeField] private GBGLifetimeScope _giantbeastghostScopePrefab;
        [SerializeField] private GiantBeastGhostSpawner _gbgspawner;

        [Header("MageGhost")]
        [SerializeField] private MGLifetimeScope _mageGhostScopePrefab;
        [SerializeField] private MageGhostSpawner _mgSpawner;

        [Header("SmallGhost")]
        [SerializeField] private SGLifetimeScope _sgScopePrefab;
        [SerializeField] private SmallGhostSpawner _sgspawner;

        [Header("Ghost King")]
        [SerializeField] private GKLifetimeScope _gkScopePrefab;
        [SerializeField] private GKSpawner _gkSpawner;

        [Header("EnemyBulletSystemRoot")]
        [SerializeField] private EnemyBulletSystemRoot _enemyBulletSystemRoot;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log($"_sgScopePrefab at configure time: {_sgScopePrefab}");

            //BulletSystem

            builder.Register<EnemyBulletSystem>(Lifetime.Singleton)
                   .AsSelf();

            builder.RegisterComponent(_enemyBulletSystemRoot)
                   .AsSelf();
            
            // ── Grid infrastructure ───────────────────────────────────────────
            // Registered here because all enemy types share one grid.
            // Child scopes (BGLifetimeScope, etc.) resolve these without
            // re-registering them — VContainer walks up the scope chain.
            builder.RegisterInstance(_gridConfig)
                   .AsSelf();

            builder.Register<SquareGrid>(Lifetime.Singleton)
                   .AsImplementedInterfaces();

            builder.Register<NodeData>(Lifetime.Singleton)
                   .AsSelf()
                   .AsImplementedInterfaces();

            builder.RegisterComponent(GetComponent<GridVisual>());

            // ── factory ──────────────────────────────────────────────
            // Singleton — one factory serves all BG spawn requests.
            // WithParameter passes the scope prefab so the factory knows which
            // prefab to instantiate and parent under this scope.
            // BigGhostStat is NOT passed here — it lives on each prefab instance.
            builder.Register<BigGhostFactory>(Lifetime.Singleton)
                   .As<IEnemyFactory<BigGhost>>()
                   .AsSelf();

            builder.Register<FireDemonFactory>(Lifetime.Singleton)
                   .WithParameter(_firedemonScopePrefab)
                   .As<IEnemyFactory<FireDemon>>();

            builder.Register<GiantBeastGhostFactory>(Lifetime.Singleton)
                   .WithParameter(_giantbeastghostScopePrefab)
                   .As<IEnemyFactory<GiantBeastGhost>>();

            builder.Register<MageGhostFactory>(Lifetime.Singleton)
                   .WithParameter(_mageGhostScopePrefab)
                   .As<IEnemyFactory<MageGhost>>();

            builder.Register<SmallGhostFactory>(Lifetime.Singleton)
                .WithParameter(_sgScopePrefab)
                .As<IEnemyFactory<SmallGhost>>()
                .AsSelf();

            builder.Register<GhostKingFactory>(Lifetime.Singleton)
                .WithParameter(_gkScopePrefab)
                .As<IEnemyFactory<GhostKing>>();
            // ── Spawner ───────────────────────────────────────────────────────
            // RegisterComponent because BigGhostSpawner is a MonoBehaviour.
            // AsImplementedInterfaces picks up IEnemySpawner or any other
            // interfaces BigGhostSpawner implements.
            builder.RegisterComponent(_bgspawner)
                   .AsSelf()
                   .AsImplementedInterfaces();

            builder.RegisterComponent(_fdspawner)
                   .AsSelf()
                   .AsImplementedInterfaces();

            builder.RegisterComponent(_gbgspawner)
                   .AsSelf()
                   .AsImplementedInterfaces();

            builder.RegisterComponent(_mgSpawner)
                  .AsSelf()
                  .AsImplementedInterfaces();
            // GKSpawner: registered but NOT auto-started — OnReady is a no-op.
            // Your arena interaction point calls GKSpawner.SpawnAt() directly.
            builder.RegisterComponent(_gkSpawner)
                   .AsSelf()
                   .AsImplementedInterfaces();
        }
    }
}
