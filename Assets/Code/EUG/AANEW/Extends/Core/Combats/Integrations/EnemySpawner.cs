using GDCO.Core.Entities;
using GDCO.Combat.Entities;
using GDCO.Combat.Damage;
using GDCO.Combat.Buffs;
using UnityEngine;
using VContainer;
using GDCO.Core.Events;

namespace GDCO.Combat.Integration
{
    /// <summary>
    /// Enemy 生成器介面
    /// </summary>
    public interface IEnemySpawner
    {
        IEnemy SpawnEnemy(GameObject prefab, Vector3 position, float maxHealth);
        void DespawnEnemy(IEnemy enemy, GameObject gameObject);
    }

    /// <summary>
    /// Enemy 生成器 - 負責 Unity GameObject 和 Pure C# Entity 的組裝
    /// 這是 Unity Integration Layer 的核心
    /// </summary>
    public class EnemySpawner : IEnemySpawner
    {
        private readonly IEntityIdProvider _idProvider;
        private readonly IEntityRegistry _entityRegistry;
        private readonly IEnemyCollisionRegistry _collisionRegistry;
        private readonly IEventBus _eventBus;

        [Inject]
        public EnemySpawner(
            IEntityIdProvider idProvider,
            IEntityRegistry entityRegistry,
            IEnemyCollisionRegistry collisionRegistry,
            IEventBus eventBus)
        {
            _idProvider = idProvider;
            _entityRegistry = entityRegistry;
            _collisionRegistry = collisionRegistry;
            _eventBus = eventBus;
        }

        public IEnemy SpawnEnemy(GameObject prefab, Vector3 position, float maxHealth)
        {
            // 1. 生成 Entity ID
            int entityId = _idProvider.Generate();

            // 2. 實例化 GameObject
            GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
            go.name = $"Enemy_{entityId}";

            // 3. 掛載 EntityBridge (方案 1)
            var bridge = go.AddComponent<EntityBridge>();
            bridge.Setup(entityId);

            // 4. 註冊碰撞查找優化 (方案 2)
            _collisionRegistry.Register(go, entityId);

            // 5. 創建 Pure C# Enemy
            // 每個 Enemy 有自己的 DamageProcessor 和 BuffContainer
            var damageProcessor = new DamageProcessor();
            var buffContainer = new BuffContainer();
            
            var enemy = new Enemy(
                entityId,
                maxHealth,
                _entityRegistry,
                _eventBus,
                damageProcessor,
                buffContainer
            );

            // 6. 初始化 (註冊到 EntityRegistry)
            enemy.Initialize();

            return enemy;
        }

        public void DespawnEnemy(IEnemy enemy, GameObject gameObject)
        {
            // 1. 清理碰撞註冊
            _collisionRegistry.Unregister(gameObject);

            // 2. 清理 Entity (會自動 unregister from EntityRegistry)
            enemy.Dispose();

            // 3. 銷毀 GameObject
            Object.Destroy(gameObject);
        }
    }
}
